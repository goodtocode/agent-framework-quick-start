using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentFramework.Presentation.Web.Options;

/// <summary>
/// Presentation.Api settings
/// </summary>
public sealed class BackendApiOptions
{
    public const string SectionName = "BackendApi";

    [Required]
    public Uri BaseUrl { get; set; } = default!;
}
