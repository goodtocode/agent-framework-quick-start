using Goodtocode.AgentFramework.Core.Domain.Playbooks;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks;

/// <summary>
/// Updates one CER step's persisted processing/action content on an existing Playbook - the
/// slow-moving query, rubric, template, or prompt a stage runs, independent of any execution.
/// </summary>
public class UpdatePlaybookStepCommand : IRequest<PlaybookDto>
{
    public required Guid PlaybookId { get; init; }
    public required PlaybookStepType StepType { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public PlaybookActionFormat? ActionFormat { get; init; }
    public string? ActionDefinition { get; init; }
}

public class UpdatePlaybookStepCommandHandler(IAgentFrameworkContext context) : IRequestHandler<UpdatePlaybookStepCommand, PlaybookDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<PlaybookDto> Handle(UpdatePlaybookStepCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var playbook = await _context.Playbooks
            .Include(x => x.Steps)
            .FirstOrDefaultAsync(x => x.Id == request.PlaybookId, cancellationToken)
            ?? throw new CustomNotFoundException($"Playbook '{request.PlaybookId}' was not found.");

        playbook.UpdateStep(request.StepType, request.Name, request.Description, request.ActionFormat, request.ActionDefinition);
        await _context.SaveChangesAsync(cancellationToken);

        return PlaybookDto.CreateFrom(playbook);
    }
}
