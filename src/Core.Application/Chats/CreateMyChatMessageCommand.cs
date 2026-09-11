using Goodtocode.AgentFramework.Core.Domain.Chats;
using Goodtocode.AgentFramework.Core.Application.Abstractions;

namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class CreateMyChatMessageCommand : UserScopedRequest, IRequest<CommandResult<ChatMessageDto>>
{
    public Guid ChatSessionId { get; set; }
    public string? Message { get; set; }
    public ChatRoutingMode RoutingMode { get; set; } = ChatRoutingMode.Routed;
    public string? IdempotencyKey { get; set; }
}

public class CreateChatMessageCommandHandler(IAgentFrameworkContext context, IChatMessageRouter routingService) : IRequestHandler<CreateMyChatMessageCommand, CommandResult<ChatMessageDto>>
{
    private readonly IAgentFrameworkContext _context = context;
    private readonly IChatMessageRouter _routingService = routingService;

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

        var requestHash = BuildRequestHash(request);
        var duplicateMessage = await TryResolveDuplicateMessageAsync(request, requestHash, cancellationToken);
        if (duplicateMessage is not null)
        {
            return CommandResult<ChatMessageDto>.Success(ChatMessageDto.CreateFrom(duplicateMessage));
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

        _context.ChatRequestIdempotency.Add(ChatRequestIdempotencyEntity.Create(
            ownerId: request.UserContext.OwnerId,
            tenantId: request.UserContext.TenantId,
            operation: ChatIdempotency.CreateMessageOperation,
            idempotencyKey: string.IsNullOrWhiteSpace(request.IdempotencyKey) ? Guid.NewGuid().ToString("N") : request.IdempotencyKey.Trim(),
            requestHash: requestHash,
            resourceId: chatMessage.Id,
            chatSessionId: chatSession.Id));

        await _context.SaveChangesAsync(cancellationToken);

        return CommandResult<ChatMessageDto>.Success(ChatMessageDto.CreateFrom(chatMessage));
    }

    private async Task<ChatMessageEntity?> TryResolveDuplicateMessageAsync(
        CreateMyChatMessageCommand request,
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
                    && x.Operation == ChatIdempotency.CreateMessageOperation
                    && x.ChatSessionId == request.ChatSessionId
                    && x.IdempotencyKey == request.IdempotencyKey)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync(cancellationToken);

            var byKeyMessage = await TryLoadMessageAsync(existingByKey?.ResourceId, ownerId, tenantId, cancellationToken);
            if (byKeyMessage is not null)
            {
                return byKeyMessage;
            }
        }

        var cutoff = now - ChatIdempotency.DuplicateWindow;
        var existingByHash = await _context.ChatRequestIdempotency
            .Where(x => x.OwnerId == ownerId
                && x.TenantId == tenantId
                && x.Operation == ChatIdempotency.CreateMessageOperation
                && x.ChatSessionId == request.ChatSessionId
                && x.RequestHash == requestHash
                && x.Timestamp >= cutoff)
            .OrderByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        return await TryLoadMessageAsync(existingByHash?.ResourceId, ownerId, tenantId, cancellationToken);
    }

    private async Task<ChatMessageEntity?> TryLoadMessageAsync(
        Guid? chatMessageId,
        Guid ownerId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!chatMessageId.HasValue)
        {
            return null;
        }

        return await _context.ChatMessages
            .FirstOrDefaultAsync(x => x.Id == chatMessageId.Value
                && x.OwnerId == ownerId
                && x.TenantId == tenantId
                && x.Role == ChatMessageRole.user, cancellationToken);
    }

    private static string BuildRequestHash(CreateMyChatMessageCommand request)
    {
        var normalizedMessage = ChatIdempotency.NormalizeMessage(request.Message);
        var routingMode = request.RoutingMode.ToString();
        return ChatIdempotency.Sha256($"{request.ChatSessionId:D}|{routingMode}|{normalizedMessage}");
    }
}
