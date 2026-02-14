using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Auth;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class CreateMyChatMessageCommand : IRequest<ChatMessageDto>, IRequiresUserContext
{
    public Guid Id { get; set; }
    public Guid ChatSessionId { get; set; }
    public string? Message { get; set; }
    public IUserContext? UserContext { get; set; }
}

public class CreateChatMessageCommandHandler(AIAgent agent, IAgentFrameworkContext context) : IRequestHandler<CreateMyChatMessageCommand, ChatMessageDto>
{
    private readonly AIAgent _agent = agent;
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ChatMessageDto> Handle(CreateMyChatMessageCommand request, CancellationToken cancellationToken)
    {
        GuardAgainstEmptyMessage(request?.Message);
        GuardAgainstIdExists(_context.ChatMessages, request!.Id);
        GuardAgainstEmptyUser(request?.UserContext);

        var chatSession = _context.ChatSessions.Find(request!.ChatSessionId);
        GuardAgainstSessionNotFound(chatSession);
        GuardAgainstUnauthorizedUser(chatSession!, request.UserContext!);

        var chatHistory = new List<ChatMessage>();
        foreach (ChatMessageEntity message in chatSession!.Messages)
        {
            chatHistory.Add(new ChatMessage(
                message.Role == ChatMessageRole.user ? ChatRole.User : ChatRole.Assistant,
                message.Content));
        }
        chatHistory.Add(new ChatMessage(ChatRole.User, request!.Message!));

        var agentResponse = await _agent.RunAsync(chatHistory, cancellationToken: cancellationToken);
        var response = agentResponse.Messages.LastOrDefault();

        GuardAgainstNullAgentResponse(response);

        var chatMessage = ChatMessageEntity.Create(
            request.Id,
            chatSession.Id,
            ChatMessageRole.user,
            request.Message!,
            request!.UserContext!.OwnerId,
            request.UserContext.TenantId
        );
        chatSession.Messages.Add(chatMessage);
        _context.ChatMessages.Add(chatMessage);

        var agentReply = (response?.Contents?.LastOrDefault()?.ToString()) ?? string.Empty;

        var chatMessageResponse = ChatMessageEntity.Create(
            Guid.NewGuid(),
            chatSession.Id,
            ChatMessageRole.assistant,
            agentReply,
            request!.UserContext!.OwnerId,
            request.UserContext.TenantId
        );
        chatSession.Messages.Add(chatMessageResponse);
        _context.ChatMessages.Add(chatMessageResponse);

        await _context.SaveChangesAsync(cancellationToken);

        return ChatMessageDto.CreateFrom(chatMessage);
    }

    private static void GuardAgainstSessionNotFound(ChatSessionEntity? chatSession)
    {
        if (chatSession == null)
            throw new CustomNotFoundException("Chat Session Not Found");
    }

    private static void GuardAgainstEmptyMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new CustomValidationException(
            [
                new("Message", "A message is required as a prompt to get an AI response")
            ]);
    }

    private static void GuardAgainstIdExists(DbSet<ChatMessageEntity> dbSet, Guid id)
    {
        if (dbSet.Any(x => x.Id == id))
            throw new CustomConflictException("Id already exists");
    }

    private static void GuardAgainstEmptyUser(IUserContext? userContext)
    {
        if (userContext == null || userContext.OwnerId == Guid.Empty || userContext.TenantId == Guid.Empty)
            throw new CustomValidationException(
            [
                new("UserContext", "User information is required to create a chat message")
            ]);
    }

    private static void GuardAgainstUnauthorizedUser(ChatSessionEntity chatSession, IUserContext userContext)
    {
        if (chatSession.OwnerId != userContext.OwnerId)
            throw new CustomForbiddenAccessException("ChatSession", chatSession.Id);
    }

    private static void GuardAgainstNullAgentResponse(ChatMessage? response)
    {
        if (response == null)
            throw new CustomValidationException([new("ChatMessage", "Agent response cannot be null")]);
    }
}
