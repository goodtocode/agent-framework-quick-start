using System.Text.Json.Serialization;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Essay;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks;
using Goodtocode.Agents.Playbook.Execution;
using Goodtocode.Agents.Playbook.Steps;
using Goodtocode.Agents.Playbook.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks.Essay;

/// <summary>
/// Agentic Evaluate stage for the Essay Rubric Evaluation example playbook: asks the model to
/// score the essay against each criterion in the versioned <see cref="EssayKnowledgeHolder"/>
/// rubric, validates the per-criterion scores against the rubric before mapping into the typed
/// <see cref="EssayRubricFinding"/> contract, then deterministically computes the rubric-weighted
/// overall score from the model's per-criterion scores.
/// </summary>
[PlaybookTool("essay.agentic.evaluate")]
public sealed class EssayEvaluateAgentTool(AIAgent agent, EssayKnowledgeHolder knowledgeHolder) : IEvaluateStepTool<EssayEvidence, EssayRubricFinding>
{
    private readonly AIAgent _agent = agent;
    private readonly PlaybookKnowledge _knowledge = knowledgeHolder.Knowledge;

    public string ToolName => "essay.agentic.evaluate";

    public async Task<EssayRubricFinding> EvaluateAsync(EssayEvidence evidence, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        var criteria = _knowledge.Rubric.Criteria;
        var criteriaList = string.Join("\n", criteria.Select(c => $"- {c.CriterionId}: {c.Description}"));
        var prompt =
            $"{_knowledge.Instruction}\n{criteriaList}\n\n" +
            $"Essay:\n{evidence.EssayText}\n\n" +
            "Respond with JSON only, matching exactly this shape:\n" +
            "{\"scores\": [{\"criterion\": \"<criterion name>\", \"score\": <0 to 1>, \"reason\": \"<short reason>\"}]}. " +
            "Include exactly one entry per criterion listed above.";

        var response = await _agent.RunAsync([new ChatMessage(ChatRole.User, prompt)], cancellationToken: cancellationToken);
        var responseText = response.Messages.LastOrDefault()?.Contents.LastOrDefault()?.ToString();

        var scoring = AgentStructuredOutputParser.ParseOrThrow<EssayScoringResponse>(responseText, ToolName);

        if (scoring.Scores is null || scoring.Scores.Count != criteria.Count)
        {
            throw new InvalidOperationException(
                $"{ToolName} received a model response scoring {scoring.Scores?.Count ?? 0} criteria, but rubric version {_knowledge.Rubric.Version} defines {criteria.Count}.");
        }

        var criterionScores = new List<EssayCriterionScore>();
        double weightedTotal = 0;

        foreach (var criterion in criteria)
        {
            var match = scoring.Scores.FirstOrDefault(s => string.Equals(s.Criterion, criterion.CriterionId, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                throw new InvalidOperationException(
                    $"{ToolName} received a model response missing a score for criterion '{criterion.CriterionId}' required by rubric version {_knowledge.Rubric.Version}.");
            }

            criterionScores.Add(new EssayCriterionScore(criterion.CriterionId, match.Score, match.Reason ?? string.Empty));
            weightedTotal += match.Score * criterion.Weight;
        }

        return new EssayRubricFinding(evidence.EssayText, criterionScores, weightedTotal, _knowledge.Rubric.Version);
    }

    private sealed class EssayScoringResponse
    {
        [JsonPropertyName("scores")]
        public List<EssayCriterionScoreResponse>? Scores { get; set; }
    }

    private sealed class EssayCriterionScoreResponse
    {
        [JsonPropertyName("criterion")]
        public string Criterion { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }
}
