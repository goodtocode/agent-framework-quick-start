namespace Goodtocode.AgentFramework.Presentation.Web.Features.Playbooks.Models;

/// <summary>
/// UI-facing projection of a <c>PlaybookDto</c>: the persisted, slow-moving catalog definition
/// of one example Playbook, including the static Collect/Evaluate/Record processing/action text
/// for each CER step. Rendered alongside the dynamic per-run <see cref="PlaybookExecutionResultModel"/>
/// so the step detail panel can show "what this step always does" next to "what happened this run".
/// </summary>
public class PlaybookCatalogModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WorkflowType { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset? ModifiedOn { get; set; }
    public IReadOnlyDictionary<PlaybookCerStep, PlaybookCatalogStepModel> Steps { get; set; } =
        new Dictionary<PlaybookCerStep, PlaybookCatalogStepModel>();

    public static PlaybookCatalogModel Create(PlaybookDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new PlaybookCatalogModel
        {
            Id = dto.Id,
            Key = dto.Key,
            Name = dto.Name,
            Description = dto.Description,
            WorkflowType = dto.WorkflowType,
            Version = dto.Version,
            CreatedOn = dto.CreatedOn,
            ModifiedOn = dto.ModifiedOn,
            Steps = dto.Steps.ToDictionary(
                step => (PlaybookCerStep)(int)step.StepType,
                PlaybookCatalogStepModel.Create)
        };
    }
}

/// <summary>
/// UI-facing projection of a <c>PlaybookStepDto</c>: one CER step's persisted name, description,
/// and processing/action definition (e.g. the T-SQL query, rubric, or projection template).
/// </summary>
public class PlaybookCatalogStepModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActionFormat { get; set; } = string.Empty;
    public string ActionDefinition { get; set; } = string.Empty;

    public static PlaybookCatalogStepModel Create(PlaybookStepDto dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        ActionFormat = dto.ActionFormat.ToString(),
        ActionDefinition = dto.ActionDefinition
    };
}
