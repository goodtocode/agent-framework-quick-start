namespace Goodtocode.AgentStart.Core.Application.Actors;

public class GetMyActorQuery : UserScopedRequest, IRequest<ActorDto?>
{
    public Guid OwnerId { get; set; }

}

public class GetActorByOwnerIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetMyActorQuery, ActorDto?>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<ActorDto?> Handle(GetMyActorQuery request, CancellationToken cancellationToken)
    {
        var actor = await _context.Actors
            .FirstOrDefaultAsync(x => x.OwnerId == request.UserContext.OwnerId && x.TenantId == request.UserContext.TenantId, cancellationToken: cancellationToken);

        return actor is null ? null : ActorDto.CreateFrom(actor);
    }
}
