using Goodtocode.AgentFramework.Core.Application.Playbooks.Persistence;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Repeatability;
using Goodtocode.Agents.Governance.Domain;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Taxonomy;

/// <summary>
/// API-facing command for running the Taxonomy Extraction and Classification example playbook end
/// to end and projecting the result into the shared <see cref="PlaybookExecutionResultDto"/> shape
/// used by every example playbook's "run" endpoint. Distinct from <see cref="ClassifyTaxonomyCommand"/>,
/// which returns the fully typed <c>PlaybookExecutionResult</c> for callers that need it.
/// <see cref="ReplayMode"/> selects the repeatability behavior: <c>Rerun</c> (default) executes
/// fresh against <see cref="Text"/>; <c>Recall</c> and <c>Replay</c> ignore <see cref="Text"/> and
/// instead rehydrate the prior execution identified by <see cref="SourceExecutionId"/> (or the
/// user's latest execution, if omitted).
/// </summary>
public sealed class RunTaxonomyPlaybookCommand : UserScopedRequest, IRequest<PlaybookExecutionResultDto>
{
    public required string Text { get; init; }
    public RepeatMode ReplayMode { get; init; } = RepeatMode.Rerun;
    public string? SourceExecutionId { get; init; }
}

public sealed class RunTaxonomyPlaybookCommandHandler(
    ISender sender,
    PlaybookRepeatCoordinator repeats,
    ITaxonomyClassificationRunner runner,
    TaxonomyStageSummarySelector summarySelector)
    : IRequestHandler<RunTaxonomyPlaybookCommand, PlaybookExecutionResultDto>
{
    public async Task<PlaybookExecutionResultDto> Handle(RunTaxonomyPlaybookCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var repeat = await repeats.PrepareAsync<TaxonomyEvidence, TaxonomyFinding>(
            "taxonomy", request.UserContext, request.ReplayMode, request.SourceExecutionId, cancellationToken);
        var text = repeat.SourceCollectInput ?? request.Text;

        var result = await runner.ClassifyAsync(text, cancellationToken, repeat.ExecutionRequest);
        var dto = PlaybookExecutionResultDtoFactory.CreateFrom(
            text, result, summarySelector, repeat.Request.Mode, repeat.SourceRecordSummary);
        dto.ExecutionId = await Persistence.PlaybookExecutionPersister.SaveAsync(sender, dto, cancellationToken);
        return dto;
    }
}
