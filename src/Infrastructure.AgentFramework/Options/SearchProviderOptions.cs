using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;

public sealed class SearchProviderOptions
{
    public const string SectionName = "SearchProvider";

    [Required]
    [RegularExpression("^(Bing|None)$")]
    public string Provider { get; set; } = "None";

    public string? Endpoint { get; set; }

    public string? ApiKey { get; set; }

    [Range(1, 50)]
    public int DefaultResultCount { get; set; } = 5;
}
