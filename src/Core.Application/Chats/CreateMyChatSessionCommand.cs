using Goodtocode.AgentFramework.Core.Domain.Actors;
using Goodtocode.AgentFramework.Core.Domain.Chats;

namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class CreateMyChatSessionCommand : UserScopedRequest, IRequest<ChatSessionDto>
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public Guid? PersonaId { get; set; }
    public int? PersonaVersion { get; set; }

}

public class CreateMyChatSessionCommandHandler(IAgentFrameworkContext context, ISender sender) : IRequestHandler<CreateMyChatSessionCommand, ChatSessionDto>
{
    private readonly IAgentFrameworkContext _context = context;
    private readonly ISender _sender = sender;

    public async Task<ChatSessionDto> Handle(CreateMyChatSessionCommand request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyMessage(request?.Message);
        ChatGuard.GuardAgainstEmptyUser(request?.UserContext);
        var message = request!.Message!;

        var actor = await _context.Actors
            .FirstOrDefaultAsync(a => a.OwnerId == request!.UserContext!.OwnerId
                && a.TenantId == request.UserContext.TenantId, cancellationToken);

        if (actor == null)
        {
            actor = ActorEntity.Create(
                ownerId: request!.UserContext.OwnerId,
                tenantId: request.UserContext.TenantId,
                firstName: request.UserContext.FirstName,
                lastName: request.UserContext.LastName,
                email: request.UserContext.Email
            );
            _context.Actors.Add(actor);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var title = request.Title ?? message[..(message.Length >= 25 ? 25 : message.Length)];

        var chatSession = ChatSessionEntity.Create(
            ownerId: request.UserContext.OwnerId,
            tenantId: request.UserContext.TenantId,
            actorId: actor.Id,
            title: title,
            personaId: request.PersonaId ?? Guid.Empty,
            personaVersion: request.PersonaVersion ?? 0);
        _context.ChatSessions.Add(chatSession);

        await _context.SaveChangesAsync(cancellationToken);

        await _sender.Send(new CreateMyChatMessageCommand
        {
            ChatSessionId = chatSession.Id,
            Message = message,
            RoutingMode = ChatRoutingMode.Routed
        }, cancellationToken);

        return ChatSessionDto.CreateFrom(chatSession);
    }
}