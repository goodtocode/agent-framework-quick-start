using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;

/// <summary>
/// Azure AI Foundry model inference settings.
/// </summary>
public sealed class AzureAIFoundryOptions
{
    public const string SectionName = "AzureAIFoundry";

    [Required]
    [RegularExpression("^chat-[a-z0-9-]+$")]
    public string ChatCompletionModelId { get; set; } = string.Empty;

    /// <summary>Deployment for interactive function execution.</summary>
    [Required]
    [RegularExpression("^tool-[a-z0-9-]+$")]
    public string ToolCompletionModelId { get; set; } = string.Empty;

    /// <summary>Deployment for reasoning-intensive playbook inference.</summary>
    [Required]
    [RegularExpression("^tool-[a-z0-9-]+$")]
    public string ToolReasoningModelId { get; set; } = string.Empty;

    [Required]
    public string TextGenerationModelId { get; set; } = string.Empty;

    [Required]
    public string TextEmbeddingModelId { get; set; } = string.Empty;

    [Required]
    public string TextModerationModelId { get; set; } = string.Empty;

    [Required]
    public string ImageModelId { get; set; } = string.Empty;

    [Required]
    public string AudioModelId { get; set; } = string.Empty;

    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;
}
