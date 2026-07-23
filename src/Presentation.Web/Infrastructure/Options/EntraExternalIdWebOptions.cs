using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentFramework.Presentation.Web.Infrastructure.Options;

/// <summary>
/// Configuration values for Web app Entra External ID sign-in and downstream API access.
/// </summary>
public sealed class EntraExternalIdWebOptions
{
    /// <summary>
    /// Configuration section name used to bind Web auth settings.
    /// </summary>
    public const string SectionName = "EntraExternalId";

    /// <summary>
    /// Entra authority instance URL (for example, https://tenant.ciamlogin.com).
    /// </summary>
    [Required(ErrorMessage = "Missing required configuration value 'EntraExternalId:Instance'.")]
    public string Instance { get; set; } = string.Empty;

    /// <summary>
    /// Entra tenant identifier used during interactive authentication.
    /// </summary>
    [Required(ErrorMessage = "Missing required configuration value 'EntraExternalId:TenantId'.")]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Web application client ID used for sign-in and token acquisition.
    /// </summary>
    [Required(ErrorMessage = "Missing required configuration value 'EntraExternalId:ClientId'.")]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether authority metadata should be validated.
    /// </summary>
    public bool ValidateAuthority { get; set; } = true;

    /// <summary>
    /// Optional password reset endpoint used by the reset-password redirect route.
    /// </summary>
    public string PasswordResetUrl { get; set; } = string.Empty;
}
