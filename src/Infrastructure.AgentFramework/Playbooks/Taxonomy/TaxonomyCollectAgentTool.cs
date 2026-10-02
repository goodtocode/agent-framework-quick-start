using System.Text.Json.Serialization;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Taxonomy;
using Goodtocode.Agents.Playbook.Steps;
using Goodtocode.Agents.Playbook.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks.Taxonomy;

/// <summary>
/// Agentic Collect stage for the Taxonomy Extraction and Classification example playbook: asks the
/// model to extract candidate taxonomy terms from the source text, then validates and maps the raw
/// response into the typed <see cref="TaxonomyEvidence"/> contract before it crosses the stage
/// boundary.
/// </summary>
[PlaybookTool("taxonomy.agentic.collect")]
public sealed class TaxonomyCollectAgentTool(AIAgent agent) : ICollectStepTool<string, TaxonomyEvidence>
{
    private readonly AIAgent _agent = agent;

    public string ToolName => "taxonomy.agentic.collect";

    public async Task<TaxonomyEvidence> ExecuteAsync(string input, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var prompt =
            "Extract the key subject-matter terms from the text below. Respond with JSON only, matching " +
            "exactly this shape: {\"terms\": [\"term1\", \"term2\"]}. Do not include any other text.\n\n" +
            $"Text:\n{input}";

        var response = await _agent.RunAsync([new ChatMessage(ChatRole.User, prompt)], cancellationToken: cancellationToken);
        var responseText = response.Messages.LastOrDefault()?.Contents.LastOrDefault()?.ToString();

        var extraction = AgentStructuredOutputParser.ParseOrThrow<TaxonomyExtractionResponse>(responseText, ToolName);

        return new TaxonomyEvidence(input, extraction.Terms ?? [], DateTimeOffset.UtcNow);
    }

    private sealed class TaxonomyExtractionResponse
    {
        [JsonPropertyName("terms")]
        public List<string>? Terms { get; set; }
    }
}
