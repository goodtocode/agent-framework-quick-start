using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentStart.Presentation.Web.Options;

/// <summary>
/// Presentation.Api settings
/// </summary>
public sealed class BackendApiOptions
{
    public const string SectionName = "BackendApi";

    [Required]
    public Uri BaseUrl { get; set; } = default!;

    [Required]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Maximum seconds to wait for one Presentation.Api request. Chat requests that fall through to the LLM can take well over 10 seconds.
    /// </summary>
    [Range(10, 600)]
    public int TimeoutSeconds { get; set; } = ResilientHttpClientOptions.DefaultTimeoutSeconds;
}
