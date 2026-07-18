namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class DeleteActorByOwnerIdCommand : UserScopedRequest, IRequest
{
    public Guid OwnerId { get; set; }

}

public class DeleteActorByOwnerIdCommandHandler(IAgentFrameworkContext context) : IRequestHandler<DeleteActorByOwnerIdCommand>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task Handle(DeleteActorByOwnerIdCommand request, CancellationToken cancellationToken)
    {
        var actor = await _context.Actors.Where(x => x.OwnerId == request.OwnerId).FirstOrDefaultAsync(cancellationToken);
        ActorGuard.GuardAgainstNotFound(actor);

        _context.Actors.Remove(actor!);
        await _context.SaveChangesAsync(cancellationToken);
    }
}