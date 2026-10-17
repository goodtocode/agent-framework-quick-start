using Goodtocode.AgentFramework.Core.Application.Playbooks.Persistence;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Repeatability;
using Goodtocode.Agents.Governance.Domain;
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
    public RepeatMode ReplayMode { get; init; } = RepeatMode.Rerun;
    public string? SourceExecutionId { get; init; }
}

public sealed class RunEssayPlaybookCommandHandler(
    ISender sender,
    PlaybookRepeatCoordinator repeats,
    IEssayEvaluationRunner runner,
    EssayStageSummarySelector summarySelector)
    : IRequestHandler<RunEssayPlaybookCommand, PlaybookExecutionResultDto>
{
    public async Task<PlaybookExecutionResultDto> Handle(RunEssayPlaybookCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var repeat = await repeats.PrepareAsync<EssayEvidence, EssayRubricFinding>(
            "essay", request.UserContext, request.ReplayMode, request.SourceExecutionId, cancellationToken);
        var essayText = repeat.SourceCollectInput ?? request.EssayText;

        var result = await runner.EvaluateAsync(essayText, cancellationToken, repeat.ExecutionRequest);
        var dto = PlaybookExecutionResultDtoFactory.CreateFrom(
            essayText, result, summarySelector, repeat.Request.Mode, repeat.SourceRecordSummary);
        dto.ExecutionId = await Persistence.PlaybookExecutionPersister.SaveAsync(sender, dto, cancellationToken);
        return dto;
    }
}
