namespace Goodtocode.AgentFramework.Core.Domain.Common;

public class RequestIdempotencyEntity : SecuredEntity<RequestIdempotencyEntity>
{
    public string OperationKey { get; private set; } = string.Empty;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string RequestHash { get; private set; } = string.Empty;
    public string ResponseType { get; private set; } = string.Empty;
    public string ResponsePayload { get; private set; } = string.Empty;
    public string? ResourceType { get; private set; }
    public Guid? ResourceId { get; private set; }
    public Guid? ScopeId { get; private set; }

    protected RequestIdempotencyEntity() : base() { }

    private RequestIdempotencyEntity(
        Guid id,
        string canonicalKey,
        Guid ownerId,
        Guid tenantId,
        Guid createdBy,
        DateTime createdOn,
        DateTimeOffset timestamp,
        string operationKey,
        string idempotencyKey,
        string requestHash,
        string responseType,
        string responsePayload,
        string? resourceType,
        Guid? resourceId,
        Guid? scopeId)
        : base(id: id, partitionKey: tenantId.ToString(), rowKey: canonicalKey,
               ownerId: ownerId, tenantId: tenantId, createdBy: createdBy,
               createdOn: createdOn, timestamp: timestamp)
    {
        OperationKey = operationKey;
        IdempotencyKey = idempotencyKey;
        RequestHash = requestHash;
        ResponseType = responseType;
        ResponsePayload = responsePayload;
        ResourceType = resourceType;
        ResourceId = resourceId;
        ScopeId = scopeId;
    }

    public static RequestIdempotencyEntity Create(
        Guid ownerId,
        Guid tenantId,
        string operationKey,
        string idempotencyKey,
        string requestHash,
        string responseType,
        string responsePayload,
        string? resourceType,
        Guid? resourceId,
        Guid? scopeId)
    {
        return new RequestIdempotencyEntity(
            id: Guid.NewGuid(),
            canonicalKey: Guid.NewGuid().ToString(),
            ownerId: ownerId,
            tenantId: tenantId,
            createdBy: ownerId,
            createdOn: DateTime.UtcNow,
            timestamp: DateTimeOffset.UtcNow,
            operationKey: operationKey,
            idempotencyKey: idempotencyKey,
            requestHash: requestHash,
            responseType: responseType,
            responsePayload: responsePayload,
            resourceType: resourceType,
            resourceId: resourceId,
            scopeId: scopeId);
    }
}
