using Goodtocode.AgentFramework.Core.Application.Playbooks.Persistence;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Essay;

/// <summary>
/// API-facing command for running the Essay Rubric Evaluation example playbook directly against
/// raw essay text (no chat-message dependency), projecting the result into the shared
/// <see cref="PlaybookExecutionResultDto"/> shape used by every example playbook's "run" endpoint.
/// Distinct from <see cref="EvaluateEssayCommand"/>, which resolves essay text from an existing
/// chat message. <see cref="ReplayMode"/> selects the repeatability behavior: <c>Rerun</c>
/// (default) executes fresh against <see cref="EssayText"/>; <c>Recall</c> and <c>Replay</c>
/// ignore <see cref="EssayText"/> and instead rehydrate the prior execution identified by
/// <see cref="SourceExecutionId"/> (or the user's latest execution, if omitted).
/// </summary>
public sealed class RunEssayPlaybookCommand : UserScopedRequest, IRequest<PlaybookExecutionResultDto>
{
    public required string EssayText { get; init; }
    public PlaybookReplayMode ReplayMode { get; init; } = PlaybookReplayMode.Rerun;
    public string? SourceExecutionId { get; init; }
}

public sealed class RunEssayPlaybookCommandHandler(
    ISender sender,
    IAgentFrameworkContext context,
    IEssayEvaluationRunner runner,
    EssayStageSummarySelector summarySelector)
    : IRequestHandler<RunEssayPlaybookCommand, PlaybookExecutionResultDto>
{
    public async Task<PlaybookExecutionResultDto> Handle(RunEssayPlaybookCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string essayText;
        PlaybookReplayContext<EssayEvidence, EssayRubricFinding>? replayContext = null;

        if (request.ReplayMode == PlaybookReplayMode.Rerun)
        {
            essayText = request.EssayText;
        }
        else
        {
            var source = await PlaybookReplaySourceResolver.ResolveAsync(
                context, "essay", request.UserContext.OwnerId, request.UserContext.TenantId,
                request.SourceExecutionId, cancellationToken);

            essayText = source.CollectInput;
            var priorEvidence = System.Text.Json.JsonSerializer.Deserialize<EssayEvidence>(source.EvidenceJson!)!;
            var priorFinding = System.Text.Json.JsonSerializer.Deserialize<EssayRubricFinding>(source.FindingJson!)!;
            replayContext = new PlaybookReplayContext<EssayEvidence, EssayRubricFinding>(
                request.ReplayMode, source.Id.ToString(), priorEvidence, priorFinding);
        }

        var result = await runner.EvaluateAsync(essayText, cancellationToken, replayContext);
        var dto = PlaybookExecutionResultDtoFactory.CreateFrom(essayText, result, summarySelector);
        dto.ExecutionId = await Persistence.PlaybookExecutionPersister.SaveAsync(sender, dto, cancellationToken);
        return dto;
    }
}
