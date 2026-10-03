namespace Goodtocode.AgentFramework.Core.Application.Playbooks;

public class DeletePlaybookCommand : IRequest<CommandResult>
{
    public required Guid Id { get; init; }
}

public class DeletePlaybookCommandHandler(IAgentFrameworkContext context) : IRequestHandler<DeletePlaybookCommand, CommandResult>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<CommandResult> Handle(DeletePlaybookCommand request, CancellationToken cancellationToken)
    {
        var playbook = await _context.Playbooks.FindAsync([request.Id, cancellationToken], cancellationToken: cancellationToken);
        if (playbook is null)
        {
            return CommandResult.NotFound();
        }

        _context.Playbooks.Remove(playbook);
        await _context.SaveChangesAsync(cancellationToken);

        return CommandResult.Success();
    }
}
