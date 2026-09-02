using Goodtocode.AgentFramework.Core.Domain.Chats;
using Goodtocode.AgentFramework.Core.Application.Abstractions;

namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class CreateMyChatMessageCommand : UserScopedRequest, IRequest<CommandResult<ChatMessageDto>>
{
    public Guid ChatSessionId { get; set; }
    public string? Message { get; set; }
    public ChatRoutingMode RoutingMode { get; set; } = ChatRoutingMode.Routed;

}

public class CreateChatMessageCommandHandler(IAgentFrameworkContext context, IChatMessageRoutingService routingService) : IRequestHandler<CreateMyChatMessageCommand, CommandResult<ChatMessageDto>>
{
    private readonly IAgentFrameworkContext _context = context;
    private readonly IChatMessageRoutingService _routingService = routingService;

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

        ChatGuard.GuardAgainstUnauthorized(chatSession, request!.UserContext!);

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
}
