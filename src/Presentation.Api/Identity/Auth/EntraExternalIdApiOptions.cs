using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentFramework.Presentation.Api.Identity.Auth;

/// <summary>
/// Configuration values for API-side Entra External ID token validation.
/// </summary>
public sealed class EntraExternalIdApiOptions
{
	/// <summary>
	/// Configuration section name used to bind API auth settings.
	/// </summary>
	public const string SectionName = "EntraExternalId";

	/// <summary>
	/// Entra authority instance URL (for example, https://tenant.ciamlogin.com).
	/// </summary>
	[Required(ErrorMessage = "Missing required configuration value 'EntraExternalId:Instance'.")]
	public string Instance { get; set; } = string.Empty;

	/// <summary>
	/// Entra tenant identifier used for token issuer validation.
	/// </summary>
	[Required(ErrorMessage = "Missing required configuration value 'EntraExternalId:TenantId'.")]
	public string TenantId { get; set; } = string.Empty;

	/// <summary>
	/// API application (resource) client ID expected as token audience.
	/// </summary>
	[Required(ErrorMessage = "Missing required configuration value 'EntraExternalId:ClientId'.")]
	public string ClientId { get; set; } = string.Empty;

	/// <summary>
	/// Indicates whether authority metadata should be validated.
	/// </summary>
	public bool ValidateAuthority { get; set; } = true;
}
