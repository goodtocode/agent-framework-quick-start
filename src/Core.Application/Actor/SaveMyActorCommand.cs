using Goodtocode.AgentFramework.Core.Domain.Actor;

namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class SaveMyActorCommand : UserScopedRequest, IRequest<ActorDto>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }

}

public class SaveMyActorCommandHandler(IAgentFrameworkContext context) : IRequestHandler<SaveMyActorCommand, ActorDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ActorDto> Handle(SaveMyActorCommand request, CancellationToken cancellationToken)
    {
        ActorGuard.GuardAgainstInvalidUserContext(request?.UserContext);

        var actor = await _context.Actors
            .Where(x => x.OwnerId == request!.UserContext.OwnerId && x.TenantId == request.UserContext.TenantId)
            .FirstOrDefaultAsync(cancellationToken);
        if (actor is not null)
        {
            actor.Update(request?.FirstName, request?.LastName, request?.Email);
            _context.Actors.Update(actor);
        }
        else
        {
            actor = ActorEntity.Create(
                ownerId: request!.UserContext.OwnerId,
                tenantId: request.UserContext.TenantId,
                firstName: request.FirstName,
                lastName: request.LastName,
                email: request.Email
            );
            _context.Actors.Add(actor);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ActorDto.CreateFrom(actor);
    }
}