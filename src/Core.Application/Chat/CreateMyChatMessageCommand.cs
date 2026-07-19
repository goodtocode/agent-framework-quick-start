using Goodtocode.AgentFramework.Core.Domain.Chat;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Core.Application.Chat;

public class CreateMyChatMessageCommand : UserScopedRequest, IRequest<ChatMessageDto>
{
    public Guid ChatSessionId { get; set; }
    public string? Message { get; set; }

}

public class CreateChatMessageCommandHandler(AIAgent agent, IAgentFrameworkContext context) : IRequestHandler<CreateMyChatMessageCommand, ChatMessageDto>
{
    private readonly AIAgent _agent = agent;
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ChatMessageDto> Handle(CreateMyChatMessageCommand request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyMessage(request?.Message);
        ChatGuard.GuardAgainstEmptyUser(request?.UserContext);

        var chatSession = await _context.ChatSessions
            .FirstOrDefaultAsync(x => x.Id == request!.ChatSessionId && x.OwnerId == request.UserContext.OwnerId && x.TenantId == request.UserContext.TenantId, cancellationToken);
        ChatGuard.GuardAgainstNotFound(chatSession);
        ChatGuard.GuardAgainstUnauthorized(chatSession!, request!.UserContext!);

        var chatHistory = new List<ChatMessage>();
        foreach (ChatMessageEntity message in chatSession!.Messages)
        {
            chatHistory.Add(new ChatMessage(
                role: message.Role == ChatMessageRole.user ? ChatRole.User : ChatRole.Assistant,
                content: message.Content));
        }
        chatHistory.Add(new ChatMessage(role: ChatRole.User, content: request!.Message!));

        var agentResponse = await _agent.RunAsync(chatHistory, cancellationToken: cancellationToken);
        var response = agentResponse.Messages.LastOrDefault();

        ChatGuard.GuardAgainstNullAgentResponse(response);

        var chatMessage = ChatMessageEntity.Create(
            ownerId: request.UserContext.OwnerId,
            tenantId: request.UserContext.TenantId,
            chatSessionId: chatSession.Id,
            role: ChatMessageRole.user,
            content: request.Message!
        );
        chatSession.Messages.Add(chatMessage);
        _context.ChatMessages.Add(chatMessage);

        var agentReply = (response?.Contents?.LastOrDefault()?.ToString()) ?? string.Empty;

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

        return ChatMessageDto.CreateFrom(chatMessage);
    }
}
