using Goodtocode.AgentFramework.Core.Application.Common.Idempotency;
using Goodtocode.AgentFramework.Core.Domain.Common;

namespace Goodtocode.AgentFramework.Core.Application.Common.Behaviors;

public sealed class IdempotencyBehavior<TRequest, TResponse>(IAgentFrameworkContext context)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestDelegateInvoker<TResponse> nextInvoker,
        CancellationToken cancellationToken)
    {
        if (request is not IIdempotentRequest idempotentRequest
            || request is not IRequiresUserContext scopedRequest)
        {
            return await nextInvoker();
        }

        idempotentRequest.IdempotencyKey = IdempotencyDefaults.ResolveKey(idempotentRequest.IdempotencyKey);
        var operationKey = idempotentRequest.OperationKey;
        var ownerId = scopedRequest.UserContext.OwnerId;
        var tenantId = scopedRequest.UserContext.TenantId;

        var existing = await _context.RequestIdempotency
            .Where(x => x.OwnerId == ownerId
                && x.TenantId == tenantId
                && x.OperationKey == operationKey
                && x.IdempotencyKey == idempotentRequest.IdempotencyKey)
            .OrderByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null
            && existing.ResponseType == typeof(TResponse).AssemblyQualifiedName
            && !string.IsNullOrWhiteSpace(existing.ResponsePayload))
        {
            var replay = IdempotencyData.Deserialize<TResponse>(existing.ResponsePayload);
            if (replay is not null)
            {
                return replay;
            }
        }

        var response = await nextInvoker();

        var requestHash = request is IIdempotencyRequestMetadata metadata
            ? metadata.BuildRequestHash()
            : BuildRequestHash(request, operationKey);
        var scopeId = request is IIdempotencyRequestMetadata withScope
            ? withScope.ScopeId
            : null;

        _context.RequestIdempotency.Add(RequestIdempotencyEntity.Create(
            ownerId: ownerId,
            tenantId: tenantId,
            operationKey: operationKey,
            idempotencyKey: idempotentRequest.IdempotencyKey,
            requestHash: requestHash,
            responseType: typeof(TResponse).AssemblyQualifiedName ?? typeof(TResponse).FullName ?? typeof(TResponse).Name,
            responsePayload: IdempotencyData.Serialize(response),
            resourceType: null,
            resourceId: null,
            scopeId: scopeId));

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Another equivalent request may have won the unique key race; response is still valid.
        }

        return response;
    }

    private static string BuildRequestHash(TRequest request, string operationKey)
    {
        var serialized = IdempotencyData.Serialize(request);
        return IdempotencyData.Sha256($"{operationKey}|{serialized}");
    }
}
