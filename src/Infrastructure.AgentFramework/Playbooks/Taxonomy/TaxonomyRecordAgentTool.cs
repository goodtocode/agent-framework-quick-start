using System.Text.Json.Serialization;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Taxonomy;
using Goodtocode.Agents.Playbook.Steps;
using Goodtocode.Agents.Playbook.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks.Taxonomy;

/// <summary>
/// Agentic Record stage for the Taxonomy Extraction and Classification example playbook: asks the
/// model to produce a short human-readable summary of the classification, then validates and maps
/// the raw response into the typed <see cref="TaxonomyMaterialization"/> contract.
/// </summary>
[PlaybookTool("taxonomy.agentic.record")]
public sealed class TaxonomyRecordAgentTool(AIAgent agent) : IRecordStepTool<TaxonomyFinding, TaxonomyMaterialization>
{
    private readonly AIAgent _agent = agent;

    public string ToolName => "taxonomy.agentic.record";

    public async Task<TaxonomyMaterialization> RecordAsync(TaxonomyFinding finding, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(finding);

        var prompt =
            "Write a one-sentence summary of this taxonomy classification result for a governance " +
            "record. Respond with JSON only, matching exactly this shape: {\"summary\": \"<one sentence>\"}.\n\n" +
            $"Category: {finding.Category}\n" +
            $"Confidence: {finding.Confidence:0.##}\n" +
            $"Rationale: {finding.Rationale}";

        var response = await _agent.RunAsync([new ChatMessage(ChatRole.User, prompt)], cancellationToken: cancellationToken);
        var responseText = response.Messages.LastOrDefault()?.Contents.LastOrDefault()?.ToString();

        var summary = AgentStructuredOutputParser.ParseOrThrow<TaxonomySummaryResponse>(responseText, ToolName);

        return new TaxonomyMaterialization(finding.Category, finding.Confidence, summary.Summary ?? string.Empty);
    }

    private sealed class TaxonomySummaryResponse
    {
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }
    }
}
