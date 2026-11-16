namespace Goodtocode.AgentStart.Core.Application.Actors;

public class GetOurActorsQuery : UserScopedRequest, IRequest<ICollection<ActorDto>>
{
}

public class GetOurActorsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetOurActorsQuery, ICollection<ActorDto>>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<ICollection<ActorDto>> Handle(GetOurActorsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Actors
            .Where(x => x.TenantId == request.UserContext.TenantId)
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .Select(x => ActorDto.CreateFrom(x))
            .ToListAsync(cancellationToken);
    }
}