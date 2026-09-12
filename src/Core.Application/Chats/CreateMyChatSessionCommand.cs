using System.Globalization;
using Goodtocode.AgentFramework.Core.Application.Common.Idempotency;
using Goodtocode.AgentFramework.Core.Domain.Actors;
using Goodtocode.AgentFramework.Core.Domain.Chats;

namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class CreateMyChatSessionCommand : IdempotentUserScopedRequest, IRequest<ChatSessionDto>, IIdempotencyRequestMetadata
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public Guid? PersonaId { get; set; }
    public int? PersonaVersion { get; set; }
    public Guid? ScopeId => null;
    public TimeSpan? DuplicateWindow => TimeSpan.FromSeconds(5);

    public string BuildRequestHash()
    {
        var title = Title?.Trim().ToLowerInvariant() ?? string.Empty;
        var normalizedMessage = IdempotencyData.NormalizeText(Message);
        var personaId = PersonaId?.ToString("D") ?? Guid.Empty.ToString("D");
        var personaVersion = PersonaVersion?.ToString(CultureInfo.InvariantCulture) ?? "0";
        return IdempotencyData.Sha256($"{OperationKey}|{normalizedMessage}|{title}|{personaId}|{personaVersion}");
    }
}

public class CreateMyChatSessionCommandHandler(
    IAgentFrameworkContext context,
    ISender sender,
    IIdempotencyDuplicateWindowPolicy duplicateWindowPolicy) : IRequestHandler<CreateMyChatSessionCommand, ChatSessionDto>
{
    private readonly IAgentFrameworkContext _context = context;
    private readonly ISender _sender = sender;
    private readonly IIdempotencyDuplicateWindowPolicy _duplicateWindowPolicy = duplicateWindowPolicy;

    public async Task<ChatSessionDto> Handle(CreateMyChatSessionCommand request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyMessage(request?.Message);
        ChatGuard.GuardAgainstEmptyUser(request?.UserContext);
        var message = request!.Message!;

        var existingSession = await TryResolveExistingSessionAsync(request, cancellationToken);
        if (existingSession is not null)
        {
            return existingSession;
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

        return ChatSessionDto.CreateFrom(chatSession);
    }

    private async Task<ChatSessionDto?> TryResolveExistingSessionAsync(
        CreateMyChatSessionCommand request,
        CancellationToken cancellationToken)
    {
        var ownerId = request.UserContext!.OwnerId;
        var tenantId = request.UserContext.TenantId;
        var requestHash = request.BuildRequestHash();
        var duplicateWindow = _duplicateWindowPolicy.ResolveWindow(request);
        if (duplicateWindow <= TimeSpan.Zero)
        {
            return null;
        }

        var cutoff = DateTimeOffset.UtcNow - duplicateWindow;

        var existingByHash = await _context.RequestIdempotency
            .Where(x => x.OwnerId == ownerId
                && x.TenantId == tenantId
                && x.OperationKey == request.OperationKey
                && x.RequestHash == requestHash
                && x.Timestamp >= cutoff)
            .OrderByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        return existingByHash is null
            ? null
            : IdempotencyData.Deserialize<ChatSessionDto>(existingByHash.ResponsePayload);
    }
}