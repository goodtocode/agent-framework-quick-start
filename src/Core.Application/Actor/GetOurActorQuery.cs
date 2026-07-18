namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class GetOurActorQuery : UserScopedRequest, IRequest<ActorDto>
{
    public Guid ActorId { get; set; }
}

public class GetActorQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetOurActorQuery, ActorDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ActorDto> Handle(GetOurActorQuery request, CancellationToken cancellationToken)
    {
        var actor = await _context.Actors
            .FirstOrDefaultAsync(x => x.Id == request.ActorId && x.TenantId == request.UserContext.TenantId, cancellationToken: cancellationToken);
        ActorGuard.GuardAgainstNotFound(actor);

        return ActorDto.CreateFrom(actor);
    }
}