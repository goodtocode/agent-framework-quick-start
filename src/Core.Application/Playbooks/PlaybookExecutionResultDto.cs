using Goodtocode.AgentFramework.Core.Application.Playbooks.Governance;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks;

/// <summary>
/// Transport shape shared by every example Playbook's "run" endpoint: one flat DTO carrying the
/// Collect/Evaluate/Record summaries and execution metadata, regardless of each playbook's own
/// typed <c>TEvidence</c>/<c>TFinding</c>/<c>TMaterialization</c> shapes. This lets
/// Presentation.Web render all three playbooks (SQL Statistics, Taxonomy, Essay) with one shared
/// component set, matching the CER input/output convention documented in
/// <c>docs/governance/playbook-workflow-types.md</c>.
/// </summary>
public sealed class PlaybookExecutionResultDto
{
    /// <summary>
    /// The persisted <see cref="Core.Domain.Playbooks.PlaybookExecutionEntity"/>'s own id. Used as
    /// the <c>SourceExecutionId</c> of a later Recall or Replay request against this execution.
    /// </summary>
    public Guid ExecutionId { get; set; }

    public string PlaybookKey { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTimeOffset StartedUtc { get; set; }
    public DateTimeOffset CompletedUtc { get; set; }
    public string ReplayMode { get; set; } = string.Empty;
    public string? SourceExecutionId { get; set; }

    /// <summary>
    /// The plain-string Collect-stage input as submitted by the caller (database name, taxonomy
    /// source text, or essay text).
    /// </summary>
    public string CollectInput { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable summary of the Collect stage's typed evidence.
    /// </summary>
    public string CollectSummary { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable summary of the Evaluate stage's typed finding.
    /// </summary>
    public string EvaluateSummary { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable summary of the Record stage's typed materialization.
    /// </summary>
    public string RecordSummary { get; set; } = string.Empty;

    /// <summary>
    /// JSON-serialized Collect-stage typed evidence, carried only so this execution can later be
    /// used as the source of a Replay request; never rendered directly by the UI.
    /// </summary>
    public string? EvidenceJson { get; set; }

    /// <summary>
    /// JSON-serialized Evaluate-stage typed finding, carried only so this execution can later be
    /// used as the source of a Recall request; never rendered directly by the UI.
    /// </summary>
    public string? FindingJson { get; set; }
}

/// <summary>
/// Builds a <see cref="PlaybookExecutionResultDto"/> from any example playbook's typed
/// <see cref="PlaybookExecutionResult{TEvidence,TFinding,TMaterialization}"/>, reusing the same
/// <see cref="IPlaybookStageSummarySelector{TEvidence,TFinding,TMaterialization}"/> each playbook
/// already supplies for governance capture, so the API surface never needs a playbook-specific DTO.
/// </summary>
public static class PlaybookExecutionResultDtoFactory
{
    /// <summary>
    /// Projects a persisted <see cref="Core.Domain.Playbooks.PlaybookExecutionEntity"/> back into
    /// the same <see cref="PlaybookExecutionResultDto"/> shape a fresh run returns, so the UI can
    /// render a prior execution (e.g. "latest execution summary") identically to a just-completed
    /// one.
    /// </summary>
    public static PlaybookExecutionResultDto CreateFrom(Core.Domain.Playbooks.PlaybookExecutionEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new PlaybookExecutionResultDto
        {
            ExecutionId = entity.Id,
            PlaybookKey = entity.PlaybookKey,
            Version = entity.PlaybookVersion,
            StartedUtc = entity.StartedUtc,
            CompletedUtc = entity.CompletedUtc,
            ReplayMode = entity.ReplayMode,
            SourceExecutionId = entity.SourceExecutionId,
            CollectInput = entity.CollectInput,
            CollectSummary = entity.CollectOutput,
            EvaluateSummary = entity.EvaluateOutput,
            RecordSummary = entity.RecordOutput,
            EvidenceJson = entity.EvidenceJson,
            FindingJson = entity.FindingJson
        };
    }

    public static PlaybookExecutionResultDto CreateFrom<TEvidence, TFinding, TMaterialization>(
        string collectInput,
        PlaybookExecutionResult<TEvidence, TFinding, TMaterialization> result,
        IPlaybookStageSummarySelector<TEvidence, TFinding, TMaterialization> summarySelector)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(summarySelector);

        return new PlaybookExecutionResultDto
        {
            PlaybookKey = result.Metadata.PlaybookKey,
            Version = result.Metadata.Version,
            StartedUtc = result.Metadata.StartedUtc,
            CompletedUtc = result.Metadata.CompletedUtc,
            ReplayMode = result.Metadata.ReplayMode.ToString(),
            SourceExecutionId = result.Metadata.SourceExecutionId,
            CollectInput = collectInput,
            CollectSummary = summarySelector.SummarizeEvidence(result.Evidence),
            EvaluateSummary = summarySelector.SummarizeFinding(result.Finding),
            RecordSummary = summarySelector.SummarizeMaterialization(result.Materialization),
            EvidenceJson = System.Text.Json.JsonSerializer.Serialize(result.Evidence),
            FindingJson = System.Text.Json.JsonSerializer.Serialize(result.Finding)
        };
    }
}
