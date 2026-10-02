namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Governance;

/// <summary>
/// Projects a playbook's typed Collect/Evaluate/Record outputs into short, human-readable
/// summaries used as the governance prompt/evidence content for each stage. Each example
/// playbook supplies its own selector so <see cref="SharedPlaybookGovernanceActivityRecorder{TEvidence,TFinding,TMaterialization}"/>
/// can stay generic across playbooks.
/// </summary>
public interface IPlaybookStageSummarySelector<in TEvidence, in TFinding, in TMaterialization>
{
    /// <summary>
    /// Summarizes the Collect stage's typed evidence for governance capture.
    /// </summary>
    string SummarizeEvidence(TEvidence evidence);

    /// <summary>
    /// Summarizes the Evaluate stage's typed finding for governance capture.
    /// </summary>
    string SummarizeFinding(TFinding finding);

    /// <summary>
    /// Summarizes the Record stage's typed materialization for governance capture.
    /// </summary>
    string SummarizeMaterialization(TMaterialization materialization);
}
