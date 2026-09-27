using Goodtocode.Agents.Playbook.Steps;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks;

/// <summary>
/// Document Review example playbook: resolves its Collect/Evaluate/Record tools per execution via
/// the shared resolver, so a follow-up workflow can swap in an agentic Evaluate tool without
/// changing this definition.
/// </summary>
public sealed class DocumentReviewPlaybookDefinition(
    IPlaybookStepToolResolver<ReviewRequest, ReviewEvidence, ReviewEvidence, ReviewFinding, ReviewFinding, ReviewRecord> resolver)
    : IPlaybookSteps<ReviewRequest, ReviewEvidence, ReviewFinding, ReviewRecord>
{
    public string PlaybookKey => "document-review";

    public string Version => "v1";

    public ICollectStep<ReviewRequest, ReviewEvidence> Collect => resolver.ResolveCollect(null);

    public IEvaluateStep<ReviewEvidence, ReviewFinding> Evaluate => resolver.ResolveEvaluate(null);

    public IRecordStep<ReviewFinding, ReviewRecord> Record => resolver.ResolveRecord(null);
}
