using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentStart.Infrastructure.AgentFramework.Options;

public sealed class GitHubCopilotSdkOptions
{
    public const string SectionName = "GitHubCopilotSDK";

    [Required]
    public string ChatCompletionModelId { get; set; } = string.Empty;

    /// <summary>Model for interactive function execution.</summary>
    [Required]
    public string ToolCompletionModelId { get; set; } = string.Empty;

    /// <summary>Model for reasoning-intensive playbook inference.</summary>
    [Required]
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
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    [Url]
    public string Endpoint { get; set; } = "https://models.inference.ai.azure.com";
}
