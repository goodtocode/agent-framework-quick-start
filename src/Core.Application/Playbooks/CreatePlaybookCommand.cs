using Goodtocode.AgentFramework.Core.Domain.Playbooks;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks;

/// <summary>
/// Creates a new Playbook catalog entry with its three required CER steps. Playbooks are shared
/// reference data (not owner/tenant scoped), so this is a plain request with no user context.
/// </summary>
public class CreatePlaybookCommand : IRequest<PlaybookDto>
{
    public required string Key { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string WorkflowType { get; init; }
    public required string Version { get; init; }
    public required PlaybookStepInput Collect { get; init; }
    public required PlaybookStepInput Evaluate { get; init; }
    public required PlaybookStepInput Record { get; init; }
}

public sealed class PlaybookStepInput
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required PlaybookActionFormat ActionFormat { get; init; }
    public required string ActionDefinition { get; init; }
}

public class CreatePlaybookCommandHandler(IAgentFrameworkContext context) : IRequestHandler<CreatePlaybookCommand, PlaybookDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<PlaybookDto> Handle(CreatePlaybookCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var alreadyExists = await _context.Playbooks.AnyAsync(x => x.Key == request.Key, cancellationToken);
        if (alreadyExists)
        {
            throw new CustomConflictException($"A playbook with key '{request.Key}' already exists.");
        }

        var playbook = PlaybookEntity.Create(
            key: request.Key,
            name: request.Name,
            description: request.Description,
            workflowType: request.WorkflowType,
            version: request.Version,
            collect: (request.Collect.Name, request.Collect.Description, request.Collect.ActionFormat, request.Collect.ActionDefinition),
            evaluate: (request.Evaluate.Name, request.Evaluate.Description, request.Evaluate.ActionFormat, request.Evaluate.ActionDefinition),
            record: (request.Record.Name, request.Record.Description, request.Record.ActionFormat, request.Record.ActionDefinition));

        _context.Playbooks.Add(playbook);
        await _context.SaveChangesAsync(cancellationToken);

        return PlaybookDto.CreateFrom(playbook);
    }
}
