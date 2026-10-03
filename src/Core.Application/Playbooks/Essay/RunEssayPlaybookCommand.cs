namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Essay;

/// <summary>
/// API-facing command for running the Essay Rubric Evaluation example playbook directly against
/// raw essay text (no chat-message dependency), projecting the result into the shared
/// <see cref="PlaybookExecutionResultDto"/> shape used by every example playbook's "run" endpoint.
/// Distinct from <see cref="EvaluateEssayCommand"/>, which resolves essay text from an existing
/// chat message.
/// </summary>
public sealed class RunEssayPlaybookCommand : IRequest<PlaybookExecutionResultDto>
{
    public required string EssayText { get; init; }
}

public sealed class RunEssayPlaybookCommandHandler(
    IEssayEvaluationRunner runner,
    EssayStageSummarySelector summarySelector)
    : IRequestHandler<RunEssayPlaybookCommand, PlaybookExecutionResultDto>
{
    public async Task<PlaybookExecutionResultDto> Handle(RunEssayPlaybookCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await runner.EvaluateAsync(request.EssayText, cancellationToken);
        return PlaybookExecutionResultDtoFactory.CreateFrom(request.EssayText, result, summarySelector);
    }
}
