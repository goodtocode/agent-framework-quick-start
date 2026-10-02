using Goodtocode.AgentFramework.Core.Application.Playbooks.Governance;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Taxonomy;

/// <summary>
/// Shared, versioned evaluation criteria for the Taxonomy Extraction and Classification example
/// playbook, expressed using the package's generic <see cref="PlaybookKnowledge"/>/<see cref="EvaluationRubric"/>
/// shape - the exact same type every example playbook uses to supply its evaluation criteria. The
/// allowed categories are modeled as a <see cref="DiscreteEvaluationScale"/> whose named levels
/// are the taxonomy's category names and descriptions.
/// </summary>
public sealed record TaxonomyKnowledgeHolder(PlaybookKnowledge Knowledge) : IPlaybookKnowledgeHolder
{
    public static readonly TaxonomyKnowledgeHolder V1 = new(BuildV1());

    private static PlaybookKnowledge BuildV1()
    {
        var scale = new DiscreteEvaluationScale(
            "taxonomy.categories",
            [
                new EvaluationScaleLevel(0, "Finance", "Budgeting, accounting, investments, and financial reporting."),
                new EvaluationScaleLevel(1, "Healthcare", "Clinical care, medical records, and health regulations."),
                new EvaluationScaleLevel(2, "Technology", "Software, infrastructure, and engineering practices."),
                new EvaluationScaleLevel(3, "Legal", "Contracts, compliance, and regulatory matters."),
                new EvaluationScaleLevel(4, "Other", "Content that does not clearly match another category.")
            ]);

        var rubric = new EvaluationRubric(
            "taxonomy.category",
            "v1",
            [new EvaluationCriterion("Category", "The single best-matching taxonomy category.", Weight: 1.0, ScaleOverride: null)],
            scale);

        return new PlaybookKnowledge(
            "Classify the extracted terms into exactly one of the categories in the discrete scale below.",
            [],
            rubric);
    }
}

/// <summary>
/// Collect-stage output: candidate terms extracted from the source text by the agentic Collect
/// tool. The model's raw response is validated and mapped into this typed contract before it ever
/// crosses the stage boundary.
/// </summary>
public sealed record TaxonomyEvidence(string SourceText, IReadOnlyList<string> ExtractedTerms, DateTimeOffset CollectedUtc);

/// <summary>
/// Evaluate-stage output: the taxonomy category assigned to the source text, with confidence and
/// rationale, against a specific <see cref="TaxonomyKnowledgeHolder"/> rubric version.
/// </summary>
public sealed record TaxonomyFinding(string SourceText, string Category, double Confidence, string Rationale, string TaxonomyVersion);

/// <summary>
/// Record-stage output: the display/persistence-ready materialization of the taxonomy
/// classification.
/// </summary>
public sealed record TaxonomyMaterialization(string Category, double Confidence, string Summary) : IPlaybookMaterializationSummary;
