using System.Text.Json.Serialization;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Taxonomy;
using Goodtocode.Agents.Playbook.Execution;
using Goodtocode.Agents.Playbook.Steps;
using Goodtocode.Agents.Playbook.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks.Taxonomy;

/// <summary>
/// Agentic Evaluate stage for the Taxonomy Extraction and Classification example playbook: asks
/// the model to assign the evidence's extracted terms to one category from the versioned
/// <see cref="TaxonomyKnowledgeHolder"/> rubric's <see cref="DiscreteEvaluationScale"/>, then
/// validates the chosen category against that scale before mapping into the typed
/// <see cref="TaxonomyFinding"/> contract.
/// </summary>
[PlaybookTool("taxonomy.agentic.evaluate")]
public sealed class TaxonomyEvaluateAgentTool(AIAgent agent, TaxonomyKnowledgeHolder knowledgeHolder) : IEvaluateStepTool<TaxonomyEvidence, TaxonomyFinding>
{
    private readonly AIAgent _agent = agent;
    private readonly PlaybookKnowledge _knowledge = knowledgeHolder.Knowledge;

    public string ToolName => "taxonomy.agentic.evaluate";

    public async Task<TaxonomyFinding> EvaluateAsync(TaxonomyEvidence evidence, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        var scale = (DiscreteEvaluationScale)_knowledge.Rubric.Scale;
        var categoryList = string.Join("\n", scale.Levels.Select(level => $"- {level.Label}: {level.Description}"));
        var prompt =
            $"{_knowledge.Instruction}\n{categoryList}\n\n" +
            $"Extracted terms: {string.Join(", ", evidence.ExtractedTerms)}\n\n" +
            "Respond with JSON only, matching exactly this shape:\n" +
            "{\"category\": \"<one of the category names above>\", \"confidence\": <number between 0 and 1>, \"rationale\": \"<short reason>\"}";

        var response = await _agent.RunAsync([new ChatMessage(ChatRole.User, prompt)], cancellationToken: cancellationToken);
        var responseText = response.Messages.LastOrDefault()?.Contents.LastOrDefault()?.ToString();

        var classification = AgentStructuredOutputParser.ParseOrThrow<TaxonomyClassificationResponse>(responseText, ToolName);

        var matchedLevel = scale.Levels
            .FirstOrDefault(level => string.Equals(level.Label, classification.Category, StringComparison.OrdinalIgnoreCase));

        if (matchedLevel is null)
        {
            throw new InvalidOperationException(
                $"{ToolName} received an unrecognized category '{classification.Category}' that is not part of taxonomy knowledge version {_knowledge.Rubric.Version}.");
        }

        return new TaxonomyFinding(evidence.SourceText, matchedLevel.Label, classification.Confidence, classification.Rationale ?? string.Empty, _knowledge.Rubric.Version);
    }

    private sealed class TaxonomyClassificationResponse
    {
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("rationale")]
        public string? Rationale { get; set; }
    }
}
