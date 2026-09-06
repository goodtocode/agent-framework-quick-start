using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;

/// <summary>
/// Azure OpenAI settings.
/// </summary>
public sealed class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";

    [Required]
    public string ChatDeploymentName { get; set; } = string.Empty;

    /// <summary>Azure OpenAI deployment used to generate semantic intent embeddings.</summary>
    public string EmbeddingDeploymentName { get; set; } = "embedding-fast";

    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;
}