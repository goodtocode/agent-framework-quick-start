using Goodtocode.AgentFramework.Core.Application.Playbooks.Taxonomy;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Execution;
using Goodtocode.Agents.Playbook.Execution;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks.Taxonomy;

/// <summary>
/// MAF-backed implementation of <see cref="ITaxonomyClassificationRunner"/>: resolves the three
/// agentic stage tools and drives them through the shared
/// <see cref="PlaybookWorkflowGraphExecutor{TCollectInput,TEvidence,TFinding,TMaterialization}"/>
/// Collect-&gt;Evaluate-&gt;Record graph.
/// </summary>
public sealed class TaxonomyClassificationRunner(
    IPlaybookStepToolResolver<string, TaxonomyEvidence, TaxonomyEvidence, TaxonomyFinding, TaxonomyFinding, TaxonomyMaterialization> resolver,
    TaxonomyGovernanceActivityRecorder recorder) : ITaxonomyClassificationRunner
{
    private static readonly PlaybookWorkflowGraphExecutor<string, TaxonomyEvidence, TaxonomyFinding, TaxonomyMaterialization> GraphExecutor = new();

    private readonly IPlaybookStepToolResolver<string, TaxonomyEvidence, TaxonomyEvidence, TaxonomyFinding, TaxonomyFinding, TaxonomyMaterialization> _resolver = resolver;
    private readonly TaxonomyGovernanceActivityRecorder _recorder = recorder;

    public Task<PlaybookExecutionResult<TaxonomyEvidence, TaxonomyFinding, TaxonomyMaterialization>> ClassifyAsync(
        string text,
        CancellationToken cancellationToken,
        PlaybookReplayContext<TaxonomyEvidence, TaxonomyFinding>? replayContext = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var identity = new PlaybookIdentity("taxonomy", "v1", Guid.NewGuid().ToString());

        return GraphExecutor.ExecuteAsync(
            identity,
            _resolver.ResolveCollect(null),
            _resolver.ResolveEvaluate(null),
            _resolver.ResolveRecord(null),
            text,
            cancellationToken,
            activityRecorder: _recorder,
            replayContext: replayContext);
    }
}
