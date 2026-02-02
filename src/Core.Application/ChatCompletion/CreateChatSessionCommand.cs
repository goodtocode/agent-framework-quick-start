using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Actor;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class CreateChatSessionCommand : IRequest<ChatSessionDto>
{
    public Guid Id { get; set; }
    public Guid ActorId { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
}

public class CreateChatSessionCommandHandler(AIAgent kernel, IAgentFrameworkContext context) : IRequestHandler<CreateChatSessionCommand, ChatSessionDto>
{
    private readonly AIAgent _agent = kernel;
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ChatSessionDto> Handle(CreateChatSessionCommand request, CancellationToken cancellationToken)
    {
        GuardAgainstEmtpyActorId(request.ActorId);
        GuardAgainstEmptyMessage(request?.Message);
        GuardAgainstIdExists(_context.ChatSessions, request!.Id);

        var chatHistory = new List<ChatMessage>
        {
            new(ChatRole.User, request.Message!)
        };

        var agentResponse = await _agent.RunAsync(chatHistory, cancellationToken: cancellationToken);
        var response = agentResponse.Messages.LastOrDefault();

        GuardAgainstNullAgentResponse(response);

        var actor = await _context.Actors
            .FirstOrDefaultAsync(x => x.OwnerId == request.ActorId, cancellationToken);
        GuardAgainstActorNotFound(actor);

        var title = request!.Title ?? $"{request!.Message![..(request.Message!.Length >= 25 ? 25 : request.Message!.Length)]}";
        var chatSession = ChatSessionEntity.Create(
            request.Id,
            actor!.Id,
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
            throw new CustomValidationException(
            [
                new("ActorId", "ActorId required for sessions")
            ]);
    }

    private static void GuardAgainstEmtpyActorId(Guid actorId)
    {
        if (actorId == Guid.Empty)
            throw new CustomValidationException(
            [
                new("ActorId", "ActorId required for sessions")
            ]);
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

    private static void GuardAgainstNullAgentResponse(object? response)
    {
        if (response == null)
            throw new CustomValidationException(
            [
                new("AgentResponse", "Failed to get a response from the AI agent")
            ]);
    }
}