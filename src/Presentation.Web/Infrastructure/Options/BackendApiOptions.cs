using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentFramework.Presentation.Web.Infrastructure.Options;

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
}
