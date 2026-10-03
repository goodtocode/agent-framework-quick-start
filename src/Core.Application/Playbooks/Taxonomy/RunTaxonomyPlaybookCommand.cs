namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Taxonomy;

/// <summary>
/// API-facing command for running the Taxonomy Extraction and Classification example playbook end
/// to end and projecting the result into the shared <see cref="PlaybookExecutionResultDto"/> shape
/// used by every example playbook's "run" endpoint. Distinct from <see cref="ClassifyTaxonomyCommand"/>,
/// which returns the fully typed <c>PlaybookExecutionResult</c> for callers that need it.
/// </summary>
public sealed class RunTaxonomyPlaybookCommand : IRequest<PlaybookExecutionResultDto>
{
    public required string Text { get; init; }
}

public sealed class RunTaxonomyPlaybookCommandHandler(
    ISender sender,
    ITaxonomyClassificationRunner runner,
    TaxonomyStageSummarySelector summarySelector)
    : IRequestHandler<RunTaxonomyPlaybookCommand, PlaybookExecutionResultDto>
{
    public async Task<PlaybookExecutionResultDto> Handle(RunTaxonomyPlaybookCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await runner.ClassifyAsync(request.Text, cancellationToken);
        var dto = PlaybookExecutionResultDtoFactory.CreateFrom(request.Text, result, summarySelector);
        await Persistence.PlaybookExecutionPersister.SaveAsync(sender, dto, cancellationToken);
        return dto;
    }
}
