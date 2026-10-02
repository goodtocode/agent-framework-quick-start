using Goodtocode.Agents.Playbook.Execution;
using Goodtocode.Agents.Playbook.Steps;
using Microsoft.Agents.AI.Workflows;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Execution;

/// <summary>
/// Builds and runs the shared three-node Collect -&gt; Evaluate -&gt; Record Microsoft Agent
/// Framework workflow graph used by every MAF-backed example playbook (fully agentic and
/// hybrid). The graph shape never changes between examples; only the
/// <see cref="ICollectStepTool{TCollectInput,TEvidence}"/>, <see cref="IEvaluateStepTool{TEvidence,TFinding}"/>,
/// and <see cref="IRecordStepTool{TFinding,TMaterialization}"/> instances supplied to
/// <see cref="ExecuteAsync"/> vary - deterministic for the hybrid workflow, agentic for the
/// fully agentic workflow - mirroring the pattern already proven in crucible-web's
/// PipelineWorkflowStepExecutor. Each node calls the shared
/// <see cref="IPlaybookStepActivityRecorder{TEvidence,TFinding,TMaterialization}"/> after its
/// stage completes, so governance capture is identical regardless of orchestration model.
/// </summary>
public sealed class PlaybookWorkflowGraphExecutor<TCollectInput, TEvidence, TFinding, TMaterialization>
{
    public async Task<PlaybookExecutionResult<TEvidence, TFinding, TMaterialization>> ExecuteAsync(
        PlaybookIdentity identity,
        ICollectStepTool<TCollectInput, TEvidence> collectTool,
        IEvaluateStepTool<TEvidence, TFinding> evaluateTool,
        IRecordStepTool<TFinding, TMaterialization> recordTool,
        TCollectInput input,
        CancellationToken cancellationToken = default,
        IPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>? activityRecorder = null)
    {
        ArgumentNullException.ThrowIfNull(collectTool);
        ArgumentNullException.ThrowIfNull(evaluateTool);
        ArgumentNullException.ThrowIfNull(recordTool);

        var startedUtc = DateTimeOffset.UtcNow;

        var collectNode = new CollectNode(identity, collectTool, activityRecorder, cancellationToken);
        var evaluateNode = new EvaluateNode(identity, evaluateTool, activityRecorder, cancellationToken);
        var recordNode = new RecordNode(identity, recordTool, activityRecorder, cancellationToken);

        var builder = new WorkflowBuilder(collectNode);
        builder.AddEdge(collectNode, evaluateNode);
        builder.AddEdge(evaluateNode, recordNode);
        builder.WithOutputFrom(recordNode);

        var workflow = builder.Build();

        await using var run = await InProcessExecution.RunAsync(workflow, input, cancellationToken: cancellationToken).ConfigureAwait(false);

        foreach (var workflowEvent in run.NewEvents)
        {
            if (workflowEvent is WorkflowOutputEvent outputEvent && outputEvent.Is<TMaterialization>(out var materialization))
            {
                var metadata = new PlaybookExecutionMetadata(
                    identity.PlaybookKey,
                    identity.Version,
                    startedUtc,
                    DateTimeOffset.UtcNow,
                    PlaybookReplayMode.Replay,
                    identity.ExecutionId);

                return new PlaybookExecutionResult<TEvidence, TFinding, TMaterialization>(
                    collectNode.Evidence!,
                    evaluateNode.Finding!,
                    materialization,
                    metadata);
            }
        }

        throw new InvalidOperationException(
            $"Playbook workflow '{identity.PlaybookKey}' did not yield a Record-stage output.");
    }

    private sealed class CollectNode(
        PlaybookIdentity identity,
        ICollectStepTool<TCollectInput, TEvidence> tool,
        IPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>? activityRecorder,
        CancellationToken cancellationToken)
        : Executor<TCollectInput, TEvidence>("collect")
    {
        public TEvidence? Evidence { get; private set; }

        public override async ValueTask<TEvidence> HandleAsync(TCollectInput message, IWorkflowContext context, CancellationToken handlerCancellationToken = default)
        {
            var evidence = await tool.ExecuteAsync(message, cancellationToken).ConfigureAwait(false);
            Evidence = evidence;

            if (activityRecorder is not null)
            {
                await activityRecorder.OnCollectedAsync(identity, evidence, tool.ToolName, cancellationToken).ConfigureAwait(false);
            }

            return evidence;
        }
    }

    private sealed class EvaluateNode(
        PlaybookIdentity identity,
        IEvaluateStepTool<TEvidence, TFinding> tool,
        IPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>? activityRecorder,
        CancellationToken cancellationToken)
        : Executor<TEvidence, TFinding>("evaluate")
    {
        public TFinding? Finding { get; private set; }

        public override async ValueTask<TFinding> HandleAsync(TEvidence message, IWorkflowContext context, CancellationToken handlerCancellationToken = default)
        {
            var finding = await tool.EvaluateAsync(message, cancellationToken).ConfigureAwait(false);
            Finding = finding;

            if (activityRecorder is not null)
            {
                await activityRecorder.OnEvaluatedAsync(identity, finding, tool.ToolName, cancellationToken).ConfigureAwait(false);
            }

            return finding;
        }
    }

    private sealed class RecordNode(
        PlaybookIdentity identity,
        IRecordStepTool<TFinding, TMaterialization> tool,
        IPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>? activityRecorder,
        CancellationToken cancellationToken)
        : Executor<TFinding, TMaterialization>("record")
    {
        public override async ValueTask<TMaterialization> HandleAsync(TFinding message, IWorkflowContext context, CancellationToken handlerCancellationToken = default)
        {
            var materialization = await tool.RecordAsync(message, cancellationToken).ConfigureAwait(false);

            if (activityRecorder is not null)
            {
                await activityRecorder.OnRecordedAsync(identity, materialization, tool.ToolName, cancellationToken).ConfigureAwait(false);
            }

            return materialization;
        }
    }
}
