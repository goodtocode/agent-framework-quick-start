using Goodtocode.AgentFramework.Core.Application.Playbooks.Governance;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;

/// <summary>
/// SQL Statistics Classification example playbook: database size classification bands.
/// </summary>
public enum SqlDatabaseSizeClassification
{
    Small,
    Medium,
    Large
}

/// <summary>
/// Collect-stage output: a point-in-time snapshot of a SQL Server database's size and object
/// counts, retrieved deterministically (no LLM involvement) through <see cref="SqlDatabaseStatisticsQuery"/>.
/// </summary>
public sealed record SqlDatabaseStatisticsEvidence(
    string DatabaseName,
    double SizeGb,
    int TableCount,
    int IndexCount,
    DateTimeOffset CollectedUtc);

/// <summary>
/// Shared, versioned evaluation criteria for the SQL Statistics Classification example playbook,
/// expressed using the package's generic <see cref="PlaybookKnowledge"/>/<see cref="EvaluationRubric"/>
/// shape - the exact same type every example playbook uses to supply its evaluation criteria. The
/// database-size bands are modeled as a <see cref="ContinuousEvaluationScale"/> whose named
/// entries ("Small", "Medium", "Large") are classified by ascending <c>Maximum</c> (inclusive): at
/// or below 20 GB is Small, at or below 100 GB is Medium, anything larger is Large.
/// </summary>
public sealed record SqlStatisticsKnowledgeHolder(PlaybookKnowledge Knowledge) : IPlaybookKnowledgeHolder
{
    public static readonly SqlStatisticsKnowledgeHolder V1 = new(BuildV1());

    private static PlaybookKnowledge BuildV1()
    {
        var scale = new ContinuousEvaluationScale(
            "sql-statistics.database-size-gb",
            [
                new EvaluationScaleEntry("Small", Minimum: 0, Maximum: 20),
                new EvaluationScaleEntry("Medium", Minimum: 20, Maximum: 100),
                new EvaluationScaleEntry("Large", Minimum: 100, Maximum: double.MaxValue)
            ]);

        var rubric = new EvaluationRubric(
            "sql-statistics.database-size",
            "v1",
            [new EvaluationCriterion("DatabaseSize", "Classifies a database by total size in GB.", Weight: 1.0, ScaleOverride: null)],
            scale);

        return new PlaybookKnowledge(
            "Classify the collected database by total size in GB using the named size bands.",
            [],
            rubric);
    }
}

/// <summary>
/// Evaluate-stage output: the rubric-driven classification, carrying forward the evidence fields
/// the Record stage needs so Record never has to re-collect or re-evaluate.
/// </summary>
public sealed record SqlDatabaseSizeFinding(
    string DatabaseName,
    double SizeGb,
    SqlDatabaseSizeClassification Classification,
    string Reason,
    string RubricVersion);

/// <summary>
/// Record-stage output: the deterministic materialization shape suitable for live display or
/// optional persistence through <see cref="Persistence.SavePlaybookMaterializationCommand"/>.
/// </summary>
public sealed record SqlDatabaseSizeMaterialization(
    string DatabaseName,
    SqlDatabaseSizeClassification Classification,
    double SizeGb,
    string Summary) : IPlaybookMaterializationSummary;
