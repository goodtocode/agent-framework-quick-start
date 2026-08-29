using Goodtocode.AgentFramework.Core.Domain.Chats;
using Goodtocode.AgentFramework.Core.Application.Governance;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class CreateMyChatMessageCommand : UserScopedRequest, IRequest<CommandResult<ChatMessageDto>>
{
    public Guid ChatSessionId { get; set; }
    public string? Message { get; set; }

}

public class CreateChatMessageCommandHandler(AIAgent agent, IAgentFrameworkContext context, ChatGovernanceGate governanceGate) : IRequestHandler<CreateMyChatMessageCommand, CommandResult<ChatMessageDto>>
{
    private readonly AIAgent _agent = agent;
    private readonly IAgentFrameworkContext _context = context;
    private readonly ChatGovernanceGate _governanceGate = governanceGate;

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

        var governed = _governanceGate.Enforce(
            request.UserContext,
            chatSession.Id,
            request.Message!);

        var chatHistory = new List<ChatMessage>
        {
            new(ChatRole.System, governed.PromptContext.SystemInstruction)
        };
        foreach (ChatMessageEntity message in chatSession.Messages)
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

        return CommandResult<ChatMessageDto>.Success(ChatMessageDto.CreateFrom(chatMessage));
    }
}
