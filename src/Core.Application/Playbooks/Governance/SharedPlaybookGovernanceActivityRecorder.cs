using Goodtocode.AgentFramework.Core.Application.Common.Auth;
using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Governance;

/// <summary>
/// Generic <see cref="IPlaybookStepActivityRecorder{TEvidence,TFinding,TMaterialization}"/> that
/// produces a real, validated <see cref="EvaluationGovernanceRecord"/> after each CER stage for
/// any example playbook, reusing the same governance pillar shape
/// <see cref="Goodtocode.AgentFramework.Core.Application.Governance.ChatGovernanceGate"/> builds
/// for chat, so playbook execution and chat inference share one governance contract.
/// </summary>
/// <remarks>
/// <see cref="RepeatabilityRecord.DeterministicReplaySupported"/> is derived from the resolved
/// tool name's naming convention: tool names containing "deterministic" report
/// <see langword="true"/>, tool names containing "agentic" report <see langword="false"/>. Tool
/// names following neither convention default to <see langword="true"/> to preserve prior
/// behavior for playbooks (such as document-review) that predate this convention.
/// </remarks>
public class SharedPlaybookGovernanceActivityRecorder<TEvidence, TFinding, TMaterialization>(
    IRlsContext userContext,
    IRepeatabilityHashStrategy hashStrategy,
    IPlaybookStageSummarySelector<TEvidence, TFinding, TMaterialization> summarySelector)
    : IPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>
{
    private readonly List<EvaluationGovernanceRecord> _records = [];

    /// <summary>
    /// Gets the governance record produced after each stage so far, in stage order.
    /// </summary>
    public IReadOnlyList<EvaluationGovernanceRecord> Records => _records;

    public Task OnCollectedAsync(PlaybookIdentity identity, TEvidence evidence, string? toolName, CancellationToken cancellationToken = default) =>
        RecordStageAsync(identity, "collect", toolName, summarySelector.SummarizeEvidence(evidence), evidence);

    public Task OnEvaluatedAsync(PlaybookIdentity identity, TFinding finding, string? toolName, CancellationToken cancellationToken = default) =>
        RecordStageAsync(identity, "evaluate", toolName, summarySelector.SummarizeFinding(finding), finding);

    public Task OnRecordedAsync(PlaybookIdentity identity, TMaterialization materialization, string? toolName, CancellationToken cancellationToken = default) =>
        RecordStageAsync(identity, "record", toolName, summarySelector.SummarizeMaterialization(materialization), materialization);

    private Task RecordStageAsync(PlaybookIdentity identity, string stage, string? toolName, string promptContent, object? rawInput)
    {
        var correlationId = Guid.NewGuid();
        var record = new EvaluationGovernanceRecord
        {
            PolicyProfileVersion = $"playbook-{identity.PlaybookKey}-{identity.Version}",
            Observability = new ObservabilityRecord
            {
                TraceId = correlationId.ToString("N"),
                CorrelationId = correlationId,
                EvidenceRefs = [GovernanceReference.Parse($"evidence://playbook/{identity.PlaybookKey}/{stage}/{correlationId:N}")]
            },
            Repeatability = new RepeatabilityRecord
            {
                ModelRef = toolName ?? "unknown",
                ModelVersion = identity.Version,
                PromptHash = hashStrategy.ComputePromptHash(promptContent),
                InputHash = hashStrategy.ComputeInputHash(new Dictionary<string, object?> { [stage] = rawInput }),
                DeterministicReplaySupported = IsDeterministicReplaySupported(toolName)
            },
            Auditability = new AuditabilityRecord
            {
                OwnerId = userContext.OwnerId,
                TenantId = userContext.TenantId,
                PrincipalDisplay = $"owner:{userContext.OwnerId:N}",
                ToolRefs = toolName is null ? [] : [GovernanceReference.Parse($"tool://playbook/{toolName}")]
            },
            Defensibility = new DefensibilityRecord
            {
                PoliciesApplied = [GovernanceReference.Parse($"policy://playbook/{identity.PlaybookKey}-v1")],
                JustificationRefs = [GovernanceReference.Parse($"justification://playbook/{identity.PlaybookKey}-{stage}")],
                ReasoningSummary = $"Playbook '{identity.PlaybookKey}' '{stage}' stage requires an attributable, traceable, and repeatable governance envelope.",
                ConfidenceScore = null
            }
        };

        var validation = EvaluationGovernanceValidator.Validate(record);
        if (!validation.IsValid)
        {
            var issues = string.Join("; ", validation.Issues.Select(issue => $"{issue.Field}: {issue.Message}"));
            throw new InvalidOperationException($"Playbook governance record for stage '{stage}' failed validation: {issues}");
        }

        _records.Add(record);
        return Task.CompletedTask;
    }

    private static bool IsDeterministicReplaySupported(string? toolName)
    {
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return true;
        }

        if (toolName.Contains("deterministic", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (toolName.Contains("agentic", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}
