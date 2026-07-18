using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;

public sealed class AgentProviderOptions
{
    public const string SectionName = "AgentProvider";

    [Required]
    [RegularExpression("^(OpenAI|GitHubCopilotSDK|AzureOpenAI|AzureAIFoundry|Ollama)$")]
    public string Kind { get; set; } = "OpenAI";
}
