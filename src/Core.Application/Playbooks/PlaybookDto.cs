using Goodtocode.AgentFramework.Core.Domain.Playbooks;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks;

/// <summary>
/// Transport shape for one <see cref="PlaybookEntity"/>: its identity/description plus its
/// exactly-three CER <see cref="PlaybookStepDto"/> children, ordered Collect, Evaluate, Record.
/// </summary>
public class PlaybookDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WorkflowType { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public IReadOnlyList<PlaybookStepDto> Steps { get; set; } = [];

    public static PlaybookDto CreateFrom(PlaybookEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new PlaybookDto
        {
            Id = entity.Id,
            Key = entity.Key,
            Name = entity.Name,
            Description = entity.Description,
            WorkflowType = entity.WorkflowType,
            Version = entity.Version,
            CreatedOn = entity.CreatedOn,
            ModifiedOn = entity.ModifiedOn,
            Steps = entity.Steps
                .OrderBy(step => step.StepType)
                .Select(PlaybookStepDto.CreateFrom)
                .ToList()
        };
    }
}
