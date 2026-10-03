namespace Goodtocode.AgentFramework.Presentation.Web.Features.Playbooks.Models;

/// <summary>
/// UI-facing projection of <c>PlaybookExecutionResultDto</c>, the shared shape every example
/// playbook's "run" endpoint returns. One model renders all three example playbooks (SQL
/// Statistics, Taxonomy, Essay) since each supplies the same Collect/Evaluate/Record summary
/// strings and execution metadata.
/// </summary>
public class PlaybookExecutionResultModel
{
    /// <summary>
    /// The persisted execution's own id - supplied as <c>SourceExecutionId</c> on a later Recall
    /// or Replay request against this execution.
    /// </summary>
    public Guid ExecutionId { get; set; }

    public string PlaybookKey { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTimeOffset StartedUtc { get; set; }
    public DateTimeOffset CompletedUtc { get; set; }
    public string ReplayMode { get; set; } = string.Empty;
    public string? SourceExecutionId { get; set; }
    public string CollectInput { get; set; } = string.Empty;
    public string CollectSummary { get; set; } = string.Empty;
    public string EvaluateSummary { get; set; } = string.Empty;
    public string RecordSummary { get; set; } = string.Empty;

    public static PlaybookExecutionResultModel Create(PlaybookExecutionResultDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new PlaybookExecutionResultModel
        {
            ExecutionId = dto.ExecutionId,
            PlaybookKey = dto.PlaybookKey,
            Version = dto.Version,
            StartedUtc = dto.StartedUtc,
            CompletedUtc = dto.CompletedUtc,
            ReplayMode = dto.ReplayMode,
            SourceExecutionId = dto.SourceExecutionId,
            CollectInput = dto.CollectInput,
            CollectSummary = dto.CollectSummary,
            EvaluateSummary = dto.EvaluateSummary,
            RecordSummary = dto.RecordSummary
        };
    }
}
