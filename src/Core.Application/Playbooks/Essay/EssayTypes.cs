using Goodtocode.AgentFramework.Core.Application.Playbooks.Governance;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Essay;

/// <summary>
/// Shared, versioned evaluation criteria for the Essay Rubric Evaluation example playbook,
/// expressed using the package's generic <see cref="PlaybookKnowledge"/>/<see cref="EvaluationRubric"/>
/// shape - the exact same type every example playbook uses to supply its evaluation criteria. Each
/// weighted criterion scores on the same 0-1 <see cref="ContinuousEvaluationScale"/>.
/// </summary>
public sealed record EssayKnowledgeHolder(PlaybookKnowledge Knowledge) : IPlaybookKnowledgeHolder
{
    public static readonly EssayKnowledgeHolder V1 = new(BuildV1());

    private static PlaybookKnowledge BuildV1()
    {
        var scale = new ContinuousEvaluationScale(
            "essay.criterion-score-0-1",
            [new EvaluationScaleEntry("Score", Minimum: 0, Maximum: 1)]);

        var rubric = new EvaluationRubric(
            "essay.rubric",
            "v1",
            [
                new EvaluationCriterion("Thesis clarity", "The essay states a clear, arguable thesis.", Weight: 0.25, ScaleOverride: null),
                new EvaluationCriterion("Evidence and support", "Claims are backed by relevant evidence or reasoning.", Weight: 0.35, ScaleOverride: null),
                new EvaluationCriterion("Organization", "Ideas flow logically from introduction to conclusion.", Weight: 0.25, ScaleOverride: null),
                new EvaluationCriterion("Grammar and mechanics", "The writing is free of significant grammar or spelling errors.", Weight: 0.15, ScaleOverride: null)
            ],
            scale);

        return new PlaybookKnowledge(
            "Score the essay against each rubric criterion on a 0 to 1 scale.",
            [],
            rubric);
    }
}

/// <summary>
/// Collect-stage output: the essay text, timestamped when it crossed the Collect-stage boundary.
/// The Collect-stage input itself is the plain essay text string (see <see cref="IEssayEvaluationRunner.EvaluateAsync"/>),
/// matching the string-shaped Collect input convention shared by every example playbook. Chat
/// message storage, when used as the source of that text, is resolved by <see cref="EvaluateEssayCommandHandler"/>
/// before the playbook runs - no new essay-specific schema is introduced.
/// </summary>
public sealed record EssayEvidence(string EssayText, DateTimeOffset CollectedUtc);

/// <summary>
/// Evaluate-stage output: the agentic per-criterion scores and rationale against a specific
/// <see cref="EssayKnowledgeHolder"/> rubric version.
/// </summary>
public sealed record EssayCriterionScore(string Criterion, double Score, string Reason);

public sealed record EssayRubricFinding(string EssayText, IReadOnlyList<EssayCriterionScore> CriterionScores, double OverallScore, string RubricVersion);

/// <summary>
/// Record-stage output: the deterministic, display/persistence-ready scorecard, including a
/// letter-grade band computed from the overall score.
/// </summary>
public sealed record EssayScorecardMaterialization(double OverallScore, string Grade, IReadOnlyList<EssayCriterionScore> Breakdown, string Summary) : IPlaybookMaterializationSummary;
