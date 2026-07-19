using Goodtocode.AgentFramework.Core.Domain.Chat;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Core.Application.Chat;

public static class ChatGuard
{
    public static void GuardAgainstEmptyMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new CustomValidationException([
                new("Message", "A message is required as a prompt to get an AI response")
            ]);
    }

    public static void GuardAgainstIdExists(DbSet<ChatMessageEntity> dbSet, Guid id)
    {
        if (dbSet.Any(x => x.Id == id))
            throw new CustomConflictException("Id already exists");
    }

    public static void GuardAgainstEmptyUser(IUserContext? userContext)
    {
        if (userContext == null || userContext.OwnerId == Guid.Empty || userContext.TenantId == Guid.Empty)
            throw new CustomValidationException([
                new("UserContext", "User information is required to create a chat message")
            ]);
    }

    public static void GuardAgainstUnauthorized(ChatSessionEntity chatSession, IUserContext userContext)
    {
        if ((chatSession.OwnerId != userContext.OwnerId) && !(userContext.Roles?.Contains("Admin") ?? false))
            throw new CustomForbiddenAccessException("You do not have permission to access this ChatSession");
    }

    public static void GuardAgainstNullAgentResponse(ChatMessage? response)
    {
        if (response == null)
            throw new CustomValidationException([
                new("ChatMessage", "Agent response cannot be null")
            ]);
    }

    public static void GuardAgainstNullAgentResponse(object? response)
    {
        if (response == null)
            throw new CustomValidationException([
                new("AgentResponse", "Failed to get a response from the AI agent")
            ]);
    }

    public static void GuardAgainstNotFound(ChatSessionEntity? chatSession)
    {
        if (chatSession == null)
            throw new CustomNotFoundException("Chat Session not found.");
    }

    public static void GuardAgainstNotFound(ChatMessageEntity? chatMessage)
    {
        if (chatMessage == null)
            throw new CustomNotFoundException("Chat Message Not Found");
    }

    public static void GuardAgainstUnauthorized(ChatMessageEntity chatMessage, IUserContext userInfo)
    {
        if (chatMessage.ChatSession?.OwnerId != userInfo.OwnerId)
            throw new CustomForbiddenAccessException("ChatMessage", chatMessage.Id);
    }

    public static void GuardAgainstEmptyUserForQuery(IUserContext? userContext)
    {
        if (userContext == null || userContext.OwnerId == Guid.Empty || userContext.TenantId == Guid.Empty)
            throw new CustomValidationException([
                new("UserInfo", "User information is required to retrieve chat sessions")
            ]);
    }

    public static void GuardAgainstEmptyId(Guid? id)
    {
        if (id == Guid.Empty)
            throw new CustomValidationException([
                new("Id", "A valid chat session ID is required")
            ]);
    }

    public static void GuardAgainstEmptyTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new CustomValidationException([
                new("Title", "Title cannot be empty")
            ]);
    }

    public static void GuardAgainstEmptyUserForPatch(IUserContext? userContext)
    {
        if (userContext == null || userContext.OwnerId == Guid.Empty || userContext.TenantId == Guid.Empty)
            throw new CustomValidationException([
                new("UserContext", "User information is required to update a chat session")
            ]);
    }
}
