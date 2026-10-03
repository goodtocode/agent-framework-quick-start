namespace Goodtocode.AgentFramework.Core.Domain.Playbooks;

/// <summary>
/// The persisted, slow-moving "processing/action" portion of one Collect, Evaluate, or Record
/// stage of a <see cref="PlaybookEntity"/>. Inputs and outputs of a run are dynamic and recorded
/// separately on <see cref="PlaybookExecutionEntity"/>; this entity captures only what the step
/// *does* - the static query, rubric, template, or prompt - so it can be queried and updated
/// through its own API surface independent of any single execution.
/// </summary>
public class PlaybookStepEntity : DomainEntity<PlaybookStepEntity>
{
    public Guid PlaybookId { get; private set; }
    public PlaybookStepType StepType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public PlaybookActionFormat ActionFormat { get; private set; }
    public string ActionDefinition { get; private set; } = string.Empty;
    public virtual PlaybookEntity? Playbook { get; private set; }

    protected PlaybookStepEntity() : base() { }

    private PlaybookStepEntity(
        Guid id,
        DateTime createdOn,
        DateTimeOffset timestamp,
        Guid playbookId,
        PlaybookStepType stepType,
        string name,
        string description,
        PlaybookActionFormat actionFormat,
        string actionDefinition)
        : base(id: id, createdOn: createdOn, timestamp: timestamp)
    {
        PlaybookId = playbookId;
        StepType = stepType;
        Name = name;
        Description = description;
        ActionFormat = actionFormat;
        ActionDefinition = actionDefinition;
    }

    public static PlaybookStepEntity Create(
        Guid playbookId,
        PlaybookStepType stepType,
        string name,
        string description,
        PlaybookActionFormat actionFormat,
        string actionDefinition)
    {
        return new PlaybookStepEntity(
            id: Guid.NewGuid(),
            createdOn: DateTime.UtcNow,
            timestamp: DateTimeOffset.UtcNow,
            playbookId: playbookId,
            stepType: stepType,
            name: name,
            description: description,
            actionFormat: actionFormat,
            actionDefinition: actionDefinition);
    }

    public void Update(string? name, string? description, PlaybookActionFormat? actionFormat, string? actionDefinition)
    {
        Name = name ?? Name;
        Description = description ?? Description;
        ActionFormat = actionFormat ?? ActionFormat;
        ActionDefinition = actionDefinition ?? ActionDefinition;
    }
}
