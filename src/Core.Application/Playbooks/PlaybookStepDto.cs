using Goodtocode.AgentFramework.Core.Domain.Playbooks;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks;

/// <summary>
/// Transport shape for one <see cref="PlaybookStepEntity"/>: its identity plus the persisted
/// processing/action content (query, rubric, template, or prompt) for one CER stage.
/// </summary>
public class PlaybookStepDto
{
    public Guid Id { get; set; }
    public PlaybookStepType StepType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PlaybookActionFormat ActionFormat { get; set; }
    public string ActionDefinition { get; set; } = string.Empty;

    public static PlaybookStepDto CreateFrom(PlaybookStepEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new PlaybookStepDto
        {
            Id = entity.Id,
            StepType = entity.StepType,
            Name = entity.Name,
            Description = entity.Description,
            ActionFormat = entity.ActionFormat,
            ActionDefinition = entity.ActionDefinition
        };
    }
}
