namespace Goodtocode.AgentFramework.Core.Application.Playbooks;

/// <summary>
/// Updates a Playbook's name/description. Key, WorkflowType, and Version are immutable identity
/// once created; step content is updated separately through <see cref="UpdatePlaybookStepCommand"/>.
/// </summary>
public class UpdatePlaybookCommand : IRequest<PlaybookDto>
{
    public required Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
}

public class UpdatePlaybookCommandHandler(IAgentFrameworkContext context) : IRequestHandler<UpdatePlaybookCommand, PlaybookDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<PlaybookDto> Handle(UpdatePlaybookCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var playbook = await _context.Playbooks
            .Include(x => x.Steps)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new CustomNotFoundException($"Playbook '{request.Id}' was not found.");

        playbook.Update(request.Name, request.Description);
        await _context.SaveChangesAsync(cancellationToken);

        return PlaybookDto.CreateFrom(playbook);
    }
}
