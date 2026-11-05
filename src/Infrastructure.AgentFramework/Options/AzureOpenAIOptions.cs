using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;

/// <summary>
/// Azure OpenAI settings.
/// </summary>
public sealed class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";

    [Required]
    [RegularExpression("^chat-[a-z0-9-]+$")]
    public string ChatDeploymentName { get; set; } = string.Empty;

    /// <summary>Function-capable deployment for the interactive agent.</summary>
    [Required]
    [RegularExpression("^tool-[a-z0-9-]+$")]
    public string ToolDeploymentName { get; set; } = string.Empty;

    /// <summary>Reasoning deployment for playbook inference.</summary>
    [Required]
    [RegularExpression("^tool-[a-z0-9-]+$")]
    public string ToolReasoningDeploymentName { get; set; } = string.Empty;

    /// <summary>Azure OpenAI deployment used to generate semantic intent embeddings.</summary>
    [Required]
    [RegularExpression("^embedding-[a-z0-9-]+$")]
    public string EmbeddingDeploymentName { get; set; } = "embedding-standard";

    /// <summary>Vector length returned by the intent embedding deployment.</summary>
    [Range(1, 16384)]
    public int EmbeddingDimension { get; set; } = 1536;

    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;
}