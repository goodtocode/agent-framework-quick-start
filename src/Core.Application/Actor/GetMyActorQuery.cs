namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class GetMyActorQuery : UserScopedRequest, IRequest<ActorDto>
{
    public Guid OwnerId { get; set; }

}

public class GetActorByOwnerIdQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyActorQuery, ActorDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ActorDto> Handle(GetMyActorQuery request, CancellationToken cancellationToken)
    {
        var actor = await _context.Actors
            .FirstOrDefaultAsync(x => x.OwnerId == request.UserContext.OwnerId && x.TenantId == request.UserContext.TenantId, cancellationToken: cancellationToken);
        ActorGuard.GuardAgainstNotFound(actor);

        return ActorDto.CreateFrom(actor);
    }
}