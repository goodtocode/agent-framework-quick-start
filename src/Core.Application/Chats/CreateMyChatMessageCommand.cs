using Goodtocode.AgentFramework.Core.Application.Common.Idempotency;
using Goodtocode.AgentFramework.Core.Domain.Chats;

namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class CreateMyChatMessageCommand : IdempotentUserScopedRequest, IRequest<CommandResult<ChatMessageDto>>, IIdempotencyRequestMetadata
{
    public Guid ChatSessionId { get; set; }
    public string? Message { get; set; }
    public ChatRoutingMode RoutingMode { get; set; } = ChatRoutingMode.Routed;
    public Guid? ScopeId => ChatSessionId;
    public TimeSpan? DuplicateWindow => TimeSpan.FromSeconds(5);

    public string BuildRequestHash()
    {
        var normalizedMessage = IdempotencyData.NormalizeText(Message);
        var routingMode = RoutingMode.ToString();
        return IdempotencyData.Sha256($"{OperationKey}|{ChatSessionId:D}|{routingMode}|{normalizedMessage}");
    }
}

public class CreateChatMessageCommandHandler(
    IAgentFrameworkContext context,
    IChatMessageRouter routingService,
    IIdempotencyDuplicateWindowPolicy duplicateWindowPolicy) : IRequestHandler<CreateMyChatMessageCommand, CommandResult<ChatMessageDto>>
{
    private readonly IAgentFrameworkContext _context = context;
    private readonly IChatMessageRouter _routingService = routingService;
    private readonly IIdempotencyDuplicateWindowPolicy _duplicateWindowPolicy = duplicateWindowPolicy;

    public async Task<CommandResult<ChatMessageDto>> Handle(CreateMyChatMessageCommand request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyMessage(request?.Message);
        ChatGuard.GuardAgainstEmptyUser(request?.UserContext);

        var chatSession = await _context.ChatSessions
            .FirstOrDefaultAsync(x => x.Id == request!.ChatSessionId && x.OwnerId == request.UserContext.OwnerId && x.TenantId == request.UserContext.TenantId, cancellationToken);
        if (chatSession is null)
        {
            return CommandResult<ChatMessageDto>.NotFound();
        }

        ChatGuard.GuardAgainstUnauthorized(chatSession, request.UserContext!);

        var duplicateResponse = await TryResolveDuplicateMessageAsync(request, cancellationToken);
        if (duplicateResponse is not null)
        {
            return duplicateResponse;
        }

        var agentReply = await _routingService.ResolveReplyAsync(
            chatSession.Id,
            request.Message!,
            cancellationToken,
            request.RoutingMode);

        var chatMessage = ChatMessageEntity.Create(
            ownerId: request.UserContext.OwnerId,
            tenantId: request.UserContext.TenantId,
            chatSessionId: chatSession.Id,
            role: ChatMessageRole.user,
            content: request.Message!
        );
        chatSession.Messages.Add(chatMessage);
        _context.ChatMessages.Add(chatMessage);

        var chatMessageResponse = ChatMessageEntity.Create(
            ownerId: request.UserContext.OwnerId,
            tenantId: request.UserContext.TenantId,
            chatSessionId: chatSession.Id,
            role: ChatMessageRole.assistant,
            content: agentReply
        );
        chatSession.Messages.Add(chatMessageResponse);
        _context.ChatMessages.Add(chatMessageResponse);

        await _context.SaveChangesAsync(cancellationToken);

        return CommandResult<ChatMessageDto>.Success(ChatMessageDto.CreateFrom(chatMessage));
    }

    private async Task<CommandResult<ChatMessageDto>?> TryResolveDuplicateMessageAsync(
        CreateMyChatMessageCommand request,
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
                && x.ScopeId == request.ChatSessionId
                && x.RequestHash == requestHash
                && x.Timestamp >= cutoff)
            .OrderByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        return existingByHash is null
            ? null
            : IdempotencyData.Deserialize<CommandResult<ChatMessageDto>>(existingByHash.ResponsePayload);
    }
}
