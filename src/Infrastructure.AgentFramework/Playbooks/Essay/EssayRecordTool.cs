using Goodtocode.AgentFramework.Core.Application.Playbooks.Essay;
using Goodtocode.Agents.Playbook.Steps;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks.Essay;

/// <summary>
/// Deterministic Record stage for the Essay Rubric Evaluation example playbook: bands the
/// rubric-weighted overall score into a letter grade and shapes the finding into a
/// display/persistence-ready scorecard. No LLM involvement.
/// </summary>
[PlaybookTool("essay.deterministic.record")]
public sealed class EssayRecordTool : IRecordStepTool<EssayRubricFinding, EssayScorecardMaterialization>
{
    public string ToolName => "essay.deterministic.record";

    public Task<EssayScorecardMaterialization> RecordAsync(EssayRubricFinding finding, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(finding);

        var grade = BandGrade(finding.OverallScore);
        var summary = $"Essay scored {finding.OverallScore:0.00} ({grade}) against rubric {finding.RubricVersion} across {finding.CriterionScores.Count} criteria.";

        return Task.FromResult(new EssayScorecardMaterialization(finding.OverallScore, grade, finding.CriterionScores, summary));
    }

    private static string BandGrade(double overallScore) => overallScore switch
    {
        >= 0.9 => "A",
        >= 0.8 => "B",
        >= 0.7 => "C",
        >= 0.6 => "D",
        _ => "F"
    };
}
