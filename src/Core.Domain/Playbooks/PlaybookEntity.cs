namespace Goodtocode.AgentFramework.Core.Domain.Playbooks;

/// <summary>
/// A Playbook's semi-static definition: its identity plus exactly one Collect, one Evaluate, and
/// one Record <see cref="PlaybookStepEntity"/>. This is the "what a playbook is" concept - name,
/// description, and each stage's persisted processing/action - separate from
/// <see cref="PlaybookExecutionEntity"/>, which records "what happened" on one run. Playbooks are
/// shared reference data (the example catalog this template ships with), not owner/tenant scoped;
/// only executions carry per-user security.
/// </summary>
public class PlaybookEntity : DomainEntity<PlaybookEntity>
{
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string WorkflowType { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;
    public virtual ICollection<PlaybookStepEntity> Steps { get; private set; } = [];

    protected PlaybookEntity() : base() { }

    private PlaybookEntity(
        Guid id,
        DateTime createdOn,
        DateTimeOffset timestamp,
        string key,
        string name,
        string description,
        string workflowType,
        string version)
        : base(id: id, createdOn: createdOn, timestamp: timestamp)
    {
        Key = key;
        Name = name;
        Description = description;
        WorkflowType = workflowType;
        Version = version;
    }

    /// <summary>
    /// Creates a Playbook with exactly the three CER steps every playbook must have. Each
    /// <paramref name="collect"/>/<paramref name="evaluate"/>/<paramref name="record"/> tuple is
    /// (name, description, actionFormat, actionDefinition) for that stage.
    /// </summary>
    public static PlaybookEntity Create(
        string key,
        string name,
        string description,
        string workflowType,
        string version,
        (string Name, string Description, PlaybookActionFormat ActionFormat, string ActionDefinition) collect,
        (string Name, string Description, PlaybookActionFormat ActionFormat, string ActionDefinition) evaluate,
        (string Name, string Description, PlaybookActionFormat ActionFormat, string ActionDefinition) record)
    {
        var playbook = new PlaybookEntity(
            id: Guid.NewGuid(),
            createdOn: DateTime.UtcNow,
            timestamp: DateTimeOffset.UtcNow,
            key: key,
            name: name,
            description: description,
            workflowType: workflowType,
            version: version);

        playbook.Steps =
        [
            PlaybookStepEntity.Create(playbook.Id, PlaybookStepType.Collect, collect.Name, collect.Description, collect.ActionFormat, collect.ActionDefinition),
            PlaybookStepEntity.Create(playbook.Id, PlaybookStepType.Evaluate, evaluate.Name, evaluate.Description, evaluate.ActionFormat, evaluate.ActionDefinition),
            PlaybookStepEntity.Create(playbook.Id, PlaybookStepType.Record, record.Name, record.Description, record.ActionFormat, record.ActionDefinition)
        ];

        return playbook;
    }

    public void Update(string? name, string? description)
    {
        Name = name ?? Name;
        Description = description ?? Description;
    }

    public PlaybookStepEntity GetStep(PlaybookStepType stepType) =>
        Steps.SingleOrDefault(step => step.StepType == stepType)
        ?? throw new InvalidOperationException($"Playbook '{Key}' is missing its required {stepType} step.");

    public void UpdateStep(PlaybookStepType stepType, string? name, string? description, PlaybookActionFormat? actionFormat, string? actionDefinition) =>
        GetStep(stepType).Update(name, description, actionFormat, actionDefinition);
}
