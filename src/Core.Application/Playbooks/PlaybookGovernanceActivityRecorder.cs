using Goodtocode.AgentFramework.Core.Application.Common.Auth;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Governance;
using Goodtocode.Agents.Governance.Application;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks;

/// <summary>
/// Document Review example playbook's governance activity recorder: a thin specialization of
/// <see cref="SharedPlaybookGovernanceActivityRecorder{TEvidence,TFinding,TMaterialization}"/>
/// that supplies the Review* stage summaries. See <see cref="SharedPlaybookGovernanceActivityRecorder{TEvidence,TFinding,TMaterialization}"/>
/// for the governance record shape shared across all example playbooks.
/// </summary>
public sealed class PlaybookGovernanceActivityRecorder(IRlsContext userContext, IRepeatabilityHashStrategy hashStrategy)
    : SharedPlaybookGovernanceActivityRecorder<ReviewEvidence, ReviewFinding, ReviewRecord>(
        userContext,
        hashStrategy,
        new DocumentReviewStageSummarySelector())
{
    private sealed class DocumentReviewStageSummarySelector : IPlaybookStageSummarySelector<ReviewEvidence, ReviewFinding, ReviewRecord>
    {
        public string SummarizeEvidence(ReviewEvidence evidence) => evidence.Document;

        public string SummarizeFinding(ReviewFinding finding) => finding.Summary;

        public string SummarizeMaterialization(ReviewRecord materialization) => materialization.Summary;
    }
}
