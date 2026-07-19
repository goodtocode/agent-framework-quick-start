using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentFramework.Presentation.Api.Options;

/// <summary>
/// Conductor capability API settings.
/// </summary>
public sealed class ConductorApiOptions
{
    /// <summary>
    /// Configuration section name for Conductor capability API settings.
    /// </summary>
    public const string SectionName = "ConductorApi";

    /// <summary>
    /// Base URL of the Conductor capability API.
    /// </summary>
    [Required]
    public Uri BaseUrl { get; set; } = default!;
}
