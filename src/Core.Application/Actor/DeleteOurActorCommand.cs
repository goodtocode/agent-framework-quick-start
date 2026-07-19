namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class DeleteOurActorCommand : UserScopedRequest, IRequest
{
    public Guid Id { get; set; }

}

public class DeleteActorCommandHandler(IAgentFrameworkContext context) : IRequestHandler<DeleteOurActorCommand>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task Handle(DeleteOurActorCommand request, CancellationToken cancellationToken)
    {
        var Actor = _context.Actors.Find(request.Id);
        ActorGuard.GuardAgainstNotFound(Actor);

        _context.Actors.Remove(Actor!);
        await _context.SaveChangesAsync(cancellationToken);
    }
}