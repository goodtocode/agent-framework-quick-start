using System.Globalization;
using Goodtocode.AgentFramework.Core.Domain.Actors;
using Goodtocode.AgentFramework.Core.Domain.Chats;

namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class CreateMyChatSessionCommand : UserScopedRequest, IRequest<ChatSessionDto>
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public Guid? PersonaId { get; set; }
    public int? PersonaVersion { get; set; }
    public string? IdempotencyKey { get; set; }
}

public class CreateMyChatSessionCommandHandler(IAgentFrameworkContext context, ISender sender) : IRequestHandler<CreateMyChatSessionCommand, ChatSessionDto>
{
    private readonly IAgentFrameworkContext _context = context;
    private readonly ISender _sender = sender;

    public async Task<ChatSessionDto> Handle(CreateMyChatSessionCommand request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyMessage(request?.Message);
        ChatGuard.GuardAgainstEmptyUser(request?.UserContext);
        var message = request!.Message!;

        var requestHash = BuildRequestHash(request);
        var existingSession = await TryResolveExistingSessionAsync(request, requestHash, cancellationToken);
        if (existingSession is not null)
        {
            return ChatSessionDto.CreateFrom(existingSession);
        }

        var actor = await _context.Actors
            .FirstOrDefaultAsync(a => a.OwnerId == request.UserContext!.OwnerId
                && a.TenantId == request.UserContext.TenantId, cancellationToken);

        if (actor == null)
        {
            actor = ActorEntity.Create(
                ownerId: request.UserContext.OwnerId,
                tenantId: request.UserContext.TenantId,
                firstName: request.UserContext.FirstName,
                lastName: request.UserContext.LastName,
                email: request.UserContext.Email
            );
            _context.Actors.Add(actor);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var title = request.Title ?? message[..(message.Length >= 25 ? 25 : message.Length)];

        var chatSession = ChatSessionEntity.Create(
            ownerId: request.UserContext.OwnerId,
            tenantId: request.UserContext.TenantId,
            actorId: actor.Id,
            title: title,
            personaId: request.PersonaId ?? Guid.Empty,
            personaVersion: request.PersonaVersion ?? 0);
        _context.ChatSessions.Add(chatSession);

        await _context.SaveChangesAsync(cancellationToken);

        await _sender.Send(new CreateMyChatMessageCommand
        {
            ChatSessionId = chatSession.Id,
            Message = message,
            RoutingMode = ChatRoutingMode.Routed
        }, cancellationToken);

        _context.ChatRequestIdempotency.Add(ChatRequestIdempotencyEntity.Create(
            ownerId: request.UserContext.OwnerId,
            tenantId: request.UserContext.TenantId,
            operation: ChatIdempotency.CreateSessionOperation,
            idempotencyKey: string.IsNullOrWhiteSpace(request.IdempotencyKey) ? Guid.NewGuid().ToString("N") : request.IdempotencyKey.Trim(),
            requestHash: requestHash,
            resourceId: chatSession.Id,
            chatSessionId: chatSession.Id));

        await _context.SaveChangesAsync(cancellationToken);

        return ChatSessionDto.CreateFrom(chatSession);
    }

    private async Task<ChatSessionEntity?> TryResolveExistingSessionAsync(
        CreateMyChatSessionCommand request,
        string requestHash,
        CancellationToken cancellationToken)
    {
        var ownerId = request.UserContext!.OwnerId;
        var tenantId = request.UserContext.TenantId;
        var now = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existingByKey = await _context.ChatRequestIdempotency
                .Where(x => x.OwnerId == ownerId
                    && x.TenantId == tenantId
                    && x.Operation == ChatIdempotency.CreateSessionOperation
                    && x.IdempotencyKey == request.IdempotencyKey)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync(cancellationToken);

            var byKeySession = await TryLoadSessionAsync(existingByKey?.ResourceId, ownerId, tenantId, cancellationToken);
            if (byKeySession is not null)
            {
                return byKeySession;
            }
        }

        var cutoff = now - ChatIdempotency.DuplicateWindow;
        var existingByHash = await _context.ChatRequestIdempotency
            .Where(x => x.OwnerId == ownerId
                && x.TenantId == tenantId
                && x.Operation == ChatIdempotency.CreateSessionOperation
                && x.RequestHash == requestHash
                && x.Timestamp >= cutoff)
            .OrderByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        return await TryLoadSessionAsync(existingByHash?.ResourceId, ownerId, tenantId, cancellationToken);
    }

    private async Task<ChatSessionEntity?> TryLoadSessionAsync(
        Guid? chatSessionId,
        Guid ownerId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!chatSessionId.HasValue)
        {
            return null;
        }

        return await _context.ChatSessions
            .FirstOrDefaultAsync(x => x.Id == chatSessionId.Value
                && x.OwnerId == ownerId
                && x.TenantId == tenantId, cancellationToken);
    }

    private static string BuildRequestHash(CreateMyChatSessionCommand request)
    {
        var title = request.Title?.Trim().ToLowerInvariant() ?? string.Empty;
        var normalizedMessage = ChatIdempotency.NormalizeMessage(request.Message);
        var personaId = request.PersonaId?.ToString("D") ?? Guid.Empty.ToString("D");
        var personaVersion = request.PersonaVersion?.ToString(CultureInfo.InvariantCulture) ?? "0";
        return ChatIdempotency.Sha256($"{normalizedMessage}|{title}|{personaId}|{personaVersion}");
    }
}