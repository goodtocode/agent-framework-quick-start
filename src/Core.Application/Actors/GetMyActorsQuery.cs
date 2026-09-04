namespace Goodtocode.AgentFramework.Core.Application.Actors;

public class GetMyActorsQuery : UserScopedRequest, IRequest<ICollection<ActorDto>>
{
}

public class GetMyActorsQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyActorsQuery, ICollection<ActorDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ICollection<ActorDto>> Handle(GetMyActorsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Actors
            .Where(x => x.OwnerId == request.UserContext.OwnerId && x.TenantId == request.UserContext.TenantId)
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .Select(x => ActorDto.CreateFrom(x))
            .ToListAsync(cancellationToken);
    }
}