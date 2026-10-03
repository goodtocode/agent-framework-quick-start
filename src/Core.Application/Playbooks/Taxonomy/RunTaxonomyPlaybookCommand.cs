using Goodtocode.AgentFramework.Core.Application.Playbooks.Persistence;
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
    public PlaybookReplayMode ReplayMode { get; init; } = PlaybookReplayMode.Rerun;
    public string? SourceExecutionId { get; init; }
}

public sealed class RunTaxonomyPlaybookCommandHandler(
    ISender sender,
    IAgentFrameworkContext context,
    ITaxonomyClassificationRunner runner,
    TaxonomyStageSummarySelector summarySelector)
    : IRequestHandler<RunTaxonomyPlaybookCommand, PlaybookExecutionResultDto>
{
    public async Task<PlaybookExecutionResultDto> Handle(RunTaxonomyPlaybookCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string text;
        PlaybookReplayContext<TaxonomyEvidence, TaxonomyFinding>? replayContext = null;

        if (request.ReplayMode == PlaybookReplayMode.Rerun)
        {
            text = request.Text;
        }
        else
        {
            var source = await PlaybookReplaySourceResolver.ResolveAsync(
                context, "taxonomy", request.UserContext.OwnerId, request.UserContext.TenantId,
                request.SourceExecutionId, cancellationToken);

            text = source.CollectInput;
            var priorEvidence = System.Text.Json.JsonSerializer.Deserialize<TaxonomyEvidence>(source.EvidenceJson!)!;
            var priorFinding = System.Text.Json.JsonSerializer.Deserialize<TaxonomyFinding>(source.FindingJson!)!;
            replayContext = new PlaybookReplayContext<TaxonomyEvidence, TaxonomyFinding>(
                request.ReplayMode, source.Id.ToString(), priorEvidence, priorFinding);
        }

        var result = await runner.ClassifyAsync(text, cancellationToken, replayContext);
        var dto = PlaybookExecutionResultDtoFactory.CreateFrom(text, result, summarySelector);
        dto.ExecutionId = await Persistence.PlaybookExecutionPersister.SaveAsync(sender, dto, cancellationToken);
        return dto;
    }
}
