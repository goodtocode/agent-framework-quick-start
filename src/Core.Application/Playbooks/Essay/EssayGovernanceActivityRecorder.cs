using Goodtocode.AgentFramework.Core.Application.Common.Auth;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Governance;
using Goodtocode.Agents.Governance.Application;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Essay;

/// <summary>
/// Projects the Essay Rubric Evaluation playbook's CER outputs into governance summaries,
/// consumed by <see cref="SharedPlaybookGovernanceActivityRecorder{TEvidence,TFinding,TMaterialization}"/>.
/// </summary>
public sealed class EssayStageSummarySelector : IPlaybookStageSummarySelector<EssayEvidence, EssayRubricFinding, EssayScorecardMaterialization>
{
    public string SummarizeEvidence(EssayEvidence evidence) => evidence.EssayText;

    public string SummarizeFinding(EssayRubricFinding finding) =>
        $"OverallScore: {finding.OverallScore:0.##}, RubricVersion: {finding.RubricVersion}, Criteria: {finding.CriterionScores.Count}";

    public string SummarizeMaterialization(EssayScorecardMaterialization materialization) => materialization.Summary;
}

/// <summary>
/// Essay Rubric Evaluation example playbook's governance activity recorder: a thin specialization
/// of <see cref="SharedPlaybookGovernanceActivityRecorder{TEvidence,TFinding,TMaterialization}"/>.
/// Collect and Record resolve to deterministic tools; Evaluate resolves to an agentic tool, so
/// only the Evaluate governance record's <c>Repeatability.DeterministicReplaySupported</c> is false.
/// </summary>
public sealed class EssayGovernanceActivityRecorder(IRlsContext userContext, IRepeatabilityHashStrategy hashStrategy)
    : SharedPlaybookGovernanceActivityRecorder<EssayEvidence, EssayRubricFinding, EssayScorecardMaterialization>(
        userContext,
        hashStrategy,
        new EssayStageSummarySelector());
