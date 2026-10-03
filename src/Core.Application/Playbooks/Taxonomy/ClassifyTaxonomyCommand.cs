using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Taxonomy;

/// <summary>
/// Framework-agnostic abstraction over running the Taxonomy Extraction and Classification example
/// playbook. Implemented in <c>Infrastructure.AgentFramework</c> using the shared MAF 3-node
/// Collect-&gt;Evaluate-&gt;Record graph, since only that project is allowed to depend on
/// <c>Microsoft.Agents.AI.Workflows</c>. The Collect-stage input is the plain source text string,
/// matching the string-shaped Collect input convention shared by every example playbook.
/// </summary>
public interface ITaxonomyClassificationRunner
{
    Task<PlaybookExecutionResult<TaxonomyEvidence, TaxonomyFinding, TaxonomyMaterialization>> ClassifyAsync(
        string text,
        CancellationToken cancellationToken,
        PlaybookReplayContext<TaxonomyEvidence, TaxonomyFinding>? replayContext = null);
}

public sealed class ClassifyTaxonomyCommand : IRequest<PlaybookExecutionResult<TaxonomyEvidence, TaxonomyFinding, TaxonomyMaterialization>>
{
    public required string Text { get; init; }
}

public sealed class ClassifyTaxonomyCommandHandler(ITaxonomyClassificationRunner runner)
    : IRequestHandler<ClassifyTaxonomyCommand, PlaybookExecutionResult<TaxonomyEvidence, TaxonomyFinding, TaxonomyMaterialization>>
{
    private readonly ITaxonomyClassificationRunner _runner = runner;

    public Task<PlaybookExecutionResult<TaxonomyEvidence, TaxonomyFinding, TaxonomyMaterialization>> Handle(
        ClassifyTaxonomyCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _runner.ClassifyAsync(request.Text, cancellationToken);
    }
}
