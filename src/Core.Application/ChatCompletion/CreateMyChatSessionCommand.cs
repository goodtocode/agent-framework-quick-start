using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Actor;
using Goodtocode.AgentFramework.Core.Domain.Auth;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;
using Goodtocode.Domain.Entities;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class CreateMyChatSessionCommand : IRequest<ChatSessionDto>, IRequiresUserContext
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public IUserContext? UserContext { get; set; }
}

public class CreateChatSessionCommandHandler(AIAgent kernel, IAgentFrameworkContext context) : IRequestHandler<CreateMyChatSessionCommand, ChatSessionDto>
{
    private readonly AIAgent _agent = kernel;
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ChatSessionDto> Handle(CreateMyChatSessionCommand request, CancellationToken cancellationToken)
    {
        GuardAgainstEmptyMessage(request?.Message);
        GuardAgainstIdExists(_context.ChatSessions, request!.Id);
        GuardAgainstEmptyUser(request?.UserContext);

        var actor = await _context.Actors
            .FirstOrDefaultAsync(a => a.OwnerId == request!.UserContext!.OwnerId 
                && a.TenantId == request.UserContext.TenantId, cancellationToken);
        
        if (actor == null)
        {
            actor = ActorEntity.Create(
                Guid.NewGuid(),
                request?.UserContext?.FirstName, 
                request?.UserContext?.LastName, 
                request?.UserContext?.Email
            );
            _context.Actors.Add(actor);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var chatHistory = new List<ChatMessage>
        {
            new(ChatRole.User, request!.Message!)
        };

        var agentResponse = await _agent.RunAsync(chatHistory, cancellationToken: cancellationToken);
        var response = agentResponse.Messages.LastOrDefault();

        GuardAgainstNullAgentResponse(response);

        var title = request!.Title ?? $"{request!.Message![..(request.Message!.Length >= 25 ? 25 : request.Message!.Length)]}";
        var chatSession = ChatSessionEntity.Create(
            request.Id,
            actor.Id,
            title,
            Enum.TryParse<ChatMessageRole>(response!.Role.ToString().ToLowerInvariant(), out var role) ? role : ChatMessageRole.assistant,
            request.Message!,
            response.ToString()
        );
        _context.ChatSessions.Add(chatSession);
        await _context.SaveChangesAsync(cancellationToken);

        return ChatSessionDto.CreateFrom(chatSession);
    }

    private static void GuardAgainstActorNotFound(ActorEntity? actor)
    {
        if (actor == null)
            throw new CustomNotFoundException("Actor Not Found");
    }

    private static void GuardAgainstEmptyMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new CustomValidationException(
            [
                new("Message", "A message is required to get a response")
            ]);
    }

    private static void GuardAgainstIdExists(DbSet<ChatSessionEntity> dbSet, Guid id)
    {
        if (dbSet.Any(x => x.Id == id))
            throw new CustomConflictException("Id already exists");
    }

    private static void GuardAgainstEmptyUser(IUserContext? userContext)
    {
        if (userContext == null || userContext.OwnerId == Guid.Empty || userContext.TenantId == Guid.Empty)
            throw new CustomValidationException(
            [
                new("UserInfo", "User information is required to create a chat session")
            ]);
    }

    private static void GuardAgainstUnauthorizedUser(ActorEntity actor, IUserContext userContext)
    {
        if (actor.OwnerId != userContext.OwnerId)
            throw new CustomForbiddenAccessException("Actor", actor.Id);
    }

    private static void GuardAgainstNullAgentResponse(object? response)
    {
        if (response == null)
            throw new CustomValidationException(
            [
                new("AgentResponse", "Failed to get a response from the AI agent")
            ]);
    }
}