using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Auth;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class CreateChatMessageCommand : IRequest<ChatMessageDto>, IUserInfoRequest
{
    public Guid Id { get; set; }
    public Guid ChatSessionId { get; set; }
    public string? Message { get; set; }
    public IUserEntity? UserInfo { get; set; }
}

public class CreateChatMessageCommandHandler(AIAgent agent, IAgentFrameworkContext context) : IRequestHandler<CreateChatMessageCommand, ChatMessageDto>
{
    private readonly AIAgent _agent = agent;
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ChatMessageDto> Handle(CreateChatMessageCommand request, CancellationToken cancellationToken)
    {
        GuardAgainstSessionNotFound(_context.ChatSessions, request!.ChatSessionId);
        GuardAgainstEmptyMessage(request?.Message);
        GuardAgainstIdExists(_context.ChatMessages, request!.Id);
        GuardAgainstEmptyUser(request?.UserInfo);
        GuardAgainstUnauthorizedUser(_context.ChatSessions, request!.UserInfo!);

        var chatSession = _context.ChatSessions.Find(request.ChatSessionId);

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
            request.Message!
        );
        chatSession.Messages.Add(chatMessage);
        _context.ChatMessages.Add(chatMessage);

        var agentReply = (response?.Contents?.LastOrDefault()?.ToString()) ?? string.Empty;

        var chatMessageResponse = ChatMessageEntity.Create(
            Guid.NewGuid(),
            chatSession.Id,
            ChatMessageRole.assistant,
            agentReply
        );
        chatSession.Messages.Add(chatMessageResponse);
        _context.ChatMessages.Add(chatMessageResponse);

        await _context.SaveChangesAsync(cancellationToken);

        return ChatMessageDto.CreateFrom(chatMessage);
    }

    private static void GuardAgainstSessionNotFound(DbSet<ChatSessionEntity> dbSet, Guid sessionId)
    {
        if (sessionId != Guid.Empty && !dbSet.Any(x => x.Id == sessionId))
            throw new CustomValidationException(
            [
                new("ChatSessionId", "Chat Session does not exist")
            ]);
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

    private static void GuardAgainstEmptyUser(IUserEntity? userInfo)
    {
        if (userInfo == null || userInfo.OwnerId == Guid.Empty || userInfo.TenantId == Guid.Empty)
            throw new CustomValidationException(
            [
                new("UserInfo", "User information is required to create a chat message")
            ]);
    }

    private static void GuardAgainstUnauthorizedUser(DbSet<ChatSessionEntity> dbSet, IUserEntity userInfo)
    {
        bool isAuthorized = dbSet.Any(x => x.Actor != null && x.Actor.OwnerId == userInfo.OwnerId);
        if (!isAuthorized)
            throw new CustomValidationException(
            [
                new("UserInfo", "User is not authorized to create a chat message in this session")
            ]);
    }

    private static void GuardAgainstNullAgentResponse(ChatMessage? response)
    {
        if (response == null)
            throw new CustomValidationException([new("ChatMessage","Agent response cannot be null")]);
    }
}
