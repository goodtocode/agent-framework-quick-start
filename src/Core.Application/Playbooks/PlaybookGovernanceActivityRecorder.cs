using Goodtocode.AgentFramework.Core.Application.Common.Auth;
using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks;

/// <summary>
/// Produces a real, validated <see cref="EvaluationGovernanceRecord"/> after each Document Review
/// CER stage, reusing the same governance pillar shape <see cref="Governance.ChatGovernanceGate"/>
/// builds for chat, so playbook execution and chat inference share one governance contract.
/// </summary>
public sealed class PlaybookGovernanceActivityRecorder(
    IRlsContext userContext,
    IRepeatabilityHashStrategy hashStrategy)
    : IPlaybookStepActivityRecorder<ReviewEvidence, ReviewFinding, ReviewRecord>
{
    private readonly List<EvaluationGovernanceRecord> _records = [];

    /// <summary>
    /// Gets the governance record produced after each stage so far, in stage order.
    /// </summary>
    public IReadOnlyList<EvaluationGovernanceRecord> Records => _records;

    public Task OnCollectedAsync(PlaybookIdentity identity, ReviewEvidence evidence, string? toolName, CancellationToken cancellationToken = default) =>
        RecordStageAsync(identity, "collect", toolName, evidence.Document, evidence);

    public Task OnEvaluatedAsync(PlaybookIdentity identity, ReviewFinding finding, string? toolName, CancellationToken cancellationToken = default) =>
        RecordStageAsync(identity, "evaluate", toolName, finding.Summary, finding);

    public Task OnRecordedAsync(PlaybookIdentity identity, ReviewRecord materialization, string? toolName, CancellationToken cancellationToken = default) =>
        RecordStageAsync(identity, "record", toolName, materialization.Summary, materialization);

    private Task RecordStageAsync(PlaybookIdentity identity, string stage, string? toolName, string promptContent, object rawInput)
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
                DeterministicReplaySupported = true
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
                ReasoningSummary = $"Document Review '{stage}' stage requires an attributable, traceable, and repeatable governance envelope.",
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
}
