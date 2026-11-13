namespace Goodtocode.AgentStart.Core.Application.Actors;

public class DeleteOurActorCommand : UserScopedRequest, IRequest<CommandResult>
{
    public Guid Id { get; set; }

}

public class DeleteActorCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteOurActorCommand, CommandResult>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<CommandResult> Handle(DeleteOurActorCommand request, CancellationToken cancellationToken)
    {
        var actor = await _context.Actors.FindAsync([request.Id, cancellationToken], cancellationToken: cancellationToken);
        if (actor is null)
        {
            return CommandResult.NotFound();
        }

        _context.Actors.Remove(actor);
        await _context.SaveChangesAsync(cancellationToken);

        return CommandResult.Success();
    }
}
