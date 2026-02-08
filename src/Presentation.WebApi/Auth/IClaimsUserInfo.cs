namespace Goodtocode.AgentFramework.Presentation.WebApi.Auth;

/// <summary>
/// Infrastructure service for reading user identity claims from HTTP authentication context.
/// </summary>
/// <remarks>This interface provides access to claims extracted from the HTTP authentication token,
/// including user identifiers, personal information, and authorization scopes. Implementations
/// typically read from HttpContext.User claims. Property names align with Microsoft Identity claim types.</remarks>
public interface IClaimsReader
{
    /// <summary>
    /// Gets the unique identifier for the authenticated user (object identifier from IdP).
    /// </summary>
    Guid ObjectId { get; }

    /// <summary>
    /// Gets the unique identifier of the tenant associated with the authenticated user.
    /// </summary>
    Guid TenantId { get; }

    /// <summary>
    /// Gets the first name of the authenticated user.
    /// </summary>
    string FirstName { get; }

    /// <summary>
    /// Gets the last name of the authenticated user.
    /// </summary>
    string LastName { get; }

    /// <summary>
    /// Gets the email address associated with the authenticated user.
    /// </summary>
    string Email { get; }

    /// <summary>
    /// Gets the collection of roles assigned to the authenticated user.
    /// </summary>
    ICollection<string> Roles { get; }

    /// <summary>
    /// Gets the collection of OAuth scopes granted to the current request.
    /// </summary>
    ICollection<string> Scopes { get; }

    /// <summary>
    /// Gets the collection of security group identifiers associated with the authenticated user.
    /// </summary>
    ICollection<string> Groups { get; }
}
