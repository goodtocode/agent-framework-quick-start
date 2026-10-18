using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;
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
/// stage completes, so governance capture is identical regardless of orchestration model. When an
/// optional <see cref="PlaybookExecutionRequest{TEvidence,TFinding}"/> skips any stage, the full
/// graph is bypassed in favor of direct, in-order tool calls with the same skip semantics as the
/// package's own <c>PlaybookExecutor</c>.
/// </summary>
public sealed class PlaybookWorkflowGraphExecutor<TCollectInput, TEvidence, TFinding, TMaterialization>(IRepeatPolicy repeatPolicy)
{
    public async Task<PlaybookExecutionResult<TEvidence, TFinding, TMaterialization>> ExecuteAsync(
        PlaybookIdentity identity,
        ICollectStepTool<TCollectInput, TEvidence> collectTool,
        IEvaluateStepTool<TEvidence, TFinding> evaluateTool,
        IRecordStepTool<TFinding, TMaterialization> recordTool,
        TCollectInput input,
        CancellationToken cancellationToken = default,
        IPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>? activityRecorder = null,
        PlaybookExecutionRequest<TEvidence, TFinding>? request = null)
    {
        ArgumentNullException.ThrowIfNull(collectTool);
        ArgumentNullException.ThrowIfNull(evaluateTool);
        ArgumentNullException.ThrowIfNull(recordTool);

        request ??= PlaybookExecutionRequest<TEvidence, TFinding>.Default;
        var plan = repeatPolicy.Plan(request.Repeat);
        request.Validate(plan);

        var startedUtc = DateTimeOffset.UtcNow;

        // The graph only models a full run; any skipped stage uses the same skip semantics as PlaybookExecutor.
        return plan.Run == ActionType.All
            ? await ExecuteRerunAsync(identity, collectTool, evaluateTool, recordTool, input, activityRecorder, startedUtc, cancellationToken).ConfigureAwait(false)
            : await ExecuteSkippingAsync(identity, collectTool, evaluateTool, recordTool, input, request, plan, activityRecorder, startedUtc, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Fresh execution: runs the full Collect-&gt;Evaluate-&gt;Record Microsoft Agent Framework
    /// workflow graph exactly as before. A first-time execution is always a Rerun.
    /// </summary>
    private async Task<PlaybookExecutionResult<TEvidence, TFinding, TMaterialization>> ExecuteRerunAsync(
        PlaybookIdentity identity,
        ICollectStepTool<TCollectInput, TEvidence> collectTool,
        IEvaluateStepTool<TEvidence, TFinding> evaluateTool,
        IRecordStepTool<TFinding, TMaterialization> recordTool,
        TCollectInput input,
        IPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>? activityRecorder,
        DateTimeOffset startedUtc,
        CancellationToken cancellationToken)
    {
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
                    SourceExecutionId: null);

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

    /// <summary>
    /// Runs the stages the request leaves enabled as direct tool calls, in order. Skipped stages
    /// are not invoked, not recorded, and take their seed instead, so Recall is a pure read.
    /// </summary>
    private static async Task<PlaybookExecutionResult<TEvidence, TFinding, TMaterialization>> ExecuteSkippingAsync(
        PlaybookIdentity identity,
        ICollectStepTool<TCollectInput, TEvidence> collectTool,
        IEvaluateStepTool<TEvidence, TFinding> evaluateTool,
        IRecordStepTool<TFinding, TMaterialization> recordTool,
        TCollectInput input,
        PlaybookExecutionRequest<TEvidence, TFinding> request,
        RepeatPlan plan,
        IPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>? activityRecorder,
        DateTimeOffset startedUtc,
        CancellationToken cancellationToken)
    {
        TEvidence evidence;
        if (!plan.Runs(ActionType.Collect))
        {
            evidence = request.SeedEvidence!;
        }
        else
        {
            evidence = await collectTool.ExecuteAsync(input, cancellationToken).ConfigureAwait(false);
            if (activityRecorder is not null)
            {
                await activityRecorder.OnCollectedAsync(identity, evidence, collectTool.ToolName, cancellationToken).ConfigureAwait(false);
            }
        }

        TFinding finding;
        if (!plan.Runs(ActionType.Evaluate))
        {
            finding = request.SeedFinding!;
        }
        else
        {
            finding = await evaluateTool.EvaluateAsync(evidence, cancellationToken).ConfigureAwait(false);
            if (activityRecorder is not null)
            {
                await activityRecorder.OnEvaluatedAsync(identity, finding, evaluateTool.ToolName, cancellationToken).ConfigureAwait(false);
            }
        }

        TMaterialization materialization = default!;
        if (plan.Runs(ActionType.Record))
        {
            materialization = await recordTool.RecordAsync(finding, cancellationToken).ConfigureAwait(false);
            if (activityRecorder is not null)
            {
                await activityRecorder.OnRecordedAsync(identity, materialization, recordTool.ToolName, cancellationToken).ConfigureAwait(false);
            }
        }

        var metadata = new PlaybookExecutionMetadata(
            identity.PlaybookKey, identity.Version, startedUtc, DateTimeOffset.UtcNow,
            SourceExecutionId: request.Repeat.SourceExecutionId);

        return new PlaybookExecutionResult<TEvidence, TFinding, TMaterialization>(evidence, finding, materialization, metadata)
        {
            ActionsExecuted = plan.Run
        };
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
