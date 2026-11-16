using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentStart.Infrastructure.AgentFramework.Options;

/// <summary>
/// Ollama OpenAI-compatible settings.
/// </summary>
public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    [Required]
    public string ChatCompletionModelId { get; set; } = string.Empty;

    /// <summary>Function-capable model for interactive application tools.</summary>
    [Required]
    public string ToolCompletionModelId { get; set; } = string.Empty;

    /// <summary>Model for reasoning-intensive playbook inference.</summary>
    [Required]
    public string ToolReasoningModelId { get; set; } = string.Empty;

    [Required]
    public string Endpoint { get; set; } = string.Empty;

    public string ApiKey { get; set; } = "ollama";
}
