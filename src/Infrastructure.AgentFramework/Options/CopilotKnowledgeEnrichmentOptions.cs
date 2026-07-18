using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;

/// <summary>
/// Configuration for Copilot SDK-backed knowledge enrichment.
/// </summary>
public sealed class CopilotKnowledgeEnrichmentOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "CopilotKnowledgeEnrichment";

    /// <summary>
    /// Source metadata value emitted in enrichment results.
    /// </summary>
    [Required]
    public string Source { get; set; } = "copilot-sdk-agentframework";

    /// <summary>
    /// Timeout in seconds for each enrichment invocation.
    /// </summary>
    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// System prompt used to enforce deterministic JSON output.
    /// </summary>
    [Required]
    public string SystemPrompt { get; set; } = "Return only compact JSON with this shape: {\"facts\":{\"key\":\"value\"}}. Facts must be deterministic and traceable to the provided prompt. If uncertain, use an empty facts object.";
}
