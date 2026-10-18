using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;

public sealed class SearchProviderOptions
{
    public const string SectionName = "SearchProvider";

    private const string SecretPlaceholder = "<secret>";

    /// <summary>Hard feature switch. When false, web search is unavailable regardless of other settings.</summary>
    public bool Enabled { get; set; } = true;

    [Required]
    [RegularExpression("^(Bing|None)$")]
    public string Provider { get; set; } = "None";

    public string? Endpoint { get; set; }

    public string? ApiKey { get; set; }

    [Range(1, 50)]
    public int DefaultResultCount { get; set; } = 5;

    /// <summary>
    /// True only when the feature switch is on and the selected provider has a real endpoint and API key.
    /// Web search tools, intents, and instructions are exposed only when this is true.
    /// </summary>
    public bool IsAvailable => Enabled && IsBingConfigured;

    public bool IsBingConfigured =>
        Provider.Equals("Bing", StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(ApiKey)
        && !ApiKey.Equals(SecretPlaceholder, StringComparison.OrdinalIgnoreCase)
        && Uri.TryCreate(Endpoint, UriKind.Absolute, out _);
}
