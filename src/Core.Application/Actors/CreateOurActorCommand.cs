using Goodtocode.AgentFramework.Core.Domain.Actor;

namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class CreateOurActorCommand : UserScopedRequest, IRequest<ActorDto>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}

public class CreateOurActorCommandHandler(IAgentFrameworkContext context) : IRequestHandler<CreateOurActorCommand, ActorDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ActorDto> Handle(CreateOurActorCommand request, CancellationToken cancellationToken)
    {
        ActorGuard.GuardAgainstEmptyUserContext(request.UserContext);
        var actor = ActorEntity.Create(
            ownerId: request.UserContext.OwnerId,
            tenantId: request.UserContext.TenantId,
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email
        );
        _context.Actors.Add(actor);
        await _context.SaveChangesAsync(cancellationToken);
        return ActorDto.CreateFrom(actor);
    }
}