using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;

/// <summary>
/// Ollama OpenAI-compatible settings.
/// </summary>
public sealed class OllamaOptions
{
	public const string SectionName = "Ollama";

	[Required]
	public string ChatCompletionModelId { get; set; } = string.Empty;

	[Required]
	public string Endpoint { get; set; } = string.Empty;

	public string ApiKey { get; set; } = "ollama";
}
