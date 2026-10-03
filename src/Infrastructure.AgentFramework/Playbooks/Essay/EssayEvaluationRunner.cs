using Goodtocode.AgentFramework.Core.Application.Playbooks.Essay;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Execution;
using Goodtocode.Agents.Playbook.Execution;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks.Essay;

/// <summary>
/// MAF-backed implementation of <see cref="IEssayEvaluationRunner"/>: resolves the deterministic
/// Collect/Record and agentic Evaluate stage tools and drives them through the same shared
/// <see cref="PlaybookWorkflowGraphExecutor{TCollectInput,TEvidence,TFinding,TMaterialization}"/>
/// Collect-&gt;Evaluate-&gt;Record graph used by the Taxonomy playbook.
/// </summary>
public sealed class EssayEvaluationRunner(
    IPlaybookStepToolResolver<string, EssayEvidence, EssayEvidence, EssayRubricFinding, EssayRubricFinding, EssayScorecardMaterialization> resolver,
    EssayGovernanceActivityRecorder recorder) : IEssayEvaluationRunner
{
    private static readonly PlaybookWorkflowGraphExecutor<string, EssayEvidence, EssayRubricFinding, EssayScorecardMaterialization> GraphExecutor = new();

    private readonly IPlaybookStepToolResolver<string, EssayEvidence, EssayEvidence, EssayRubricFinding, EssayRubricFinding, EssayScorecardMaterialization> _resolver = resolver;
    private readonly EssayGovernanceActivityRecorder _recorder = recorder;

    public Task<PlaybookExecutionResult<EssayEvidence, EssayRubricFinding, EssayScorecardMaterialization>> EvaluateAsync(
        string essayText,
        CancellationToken cancellationToken,
        PlaybookReplayContext<EssayEvidence, EssayRubricFinding>? replayContext = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(essayText);

        var identity = new PlaybookIdentity("essay", "v1", Guid.NewGuid().ToString());

        return GraphExecutor.ExecuteAsync(
            identity,
            _resolver.ResolveCollect(null),
            _resolver.ResolveEvaluate(null),
            _resolver.ResolveRecord(null),
            essayText,
            cancellationToken,
            activityRecorder: _recorder,
            replayContext: replayContext);
    }
}
