namespace Goodtocode.AgentFramework.Core.Application.Actors;

public class DeleteActorByOwnerIdCommand : UserScopedRequest, IRequest<CommandResult>
{
    public Guid OwnerId { get; set; }

}

public class DeleteActorByOwnerIdCommandHandler(IAgentFrameworkContext context) : IRequestHandler<DeleteActorByOwnerIdCommand, CommandResult>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<CommandResult> Handle(DeleteActorByOwnerIdCommand request, CancellationToken cancellationToken)
    {
        var actor = await _context.Actors.Where(x => x.OwnerId == request.OwnerId).FirstOrDefaultAsync(cancellationToken);
        if (actor is null)
        {
            return CommandResult.NotFound();
        }

        _context.Actors.Remove(actor);
        await _context.SaveChangesAsync(cancellationToken);

        return CommandResult.Success();
    }
}
