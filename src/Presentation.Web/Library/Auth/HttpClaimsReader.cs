namespace Goodtocode.AgentFramework.Presentation.Web.Library.Auth;

/// <summary>
/// Infrastructure implementation that reads user claims from the current HTTP request context.
/// </summary>
/// <param name="contextAccessor">HttpContext containing authentication claims</param>
public class HttpClaimsReader(IHttpContextAccessor contextAccessor) : IClaimsReader
{
    private readonly HttpContext? context = contextAccessor?.HttpContext;

    private string ClaimValue(params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = context?.User.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return string.Empty;
    }

    private string FullName => ClaimValue(
        "name",
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");

    private string[] FullNameParts => FullName
        .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>
    /// Gets the unique identifier of the user object from the objectidentifier claim.
    /// </summary>
    /// <remarks>This property retrieves the value of the "objectidentifier" claim from the current user's context. 
    /// Ensure that the claim is present and properly formatted as a GUID in the authentication token.</remarks>
    public Guid ObjectId => Guid.TryParse(
        context?.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value,
        out var objectId) ? objectId : Guid.Empty;

    /// <summary>
    /// Gets the unique identifier of the tenant from the tenantid claim.
    /// </summary>
    /// <remarks>The tenant ID is extracted from the user's claims using the claim type 
    /// "http://schemas.microsoft.com/identity/claims/tenantid". Ensure that the claim is present and valid
    /// in the user's identity for this property to return a meaningful value.</remarks>
    public Guid TenantId => Guid.TryParse(
        context?.User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value,
        out var tenantId) ? tenantId : Guid.Empty;

    /// <summary>
    /// Gets the first name of the user from the givenname claim.
    /// </summary>
    public string FirstName
    {
        get
        {
            var firstName = ClaimValue(
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname",
                "given_name");
            if (!string.IsNullOrWhiteSpace(firstName))
                return firstName;

            if (FullNameParts.Length > 0)
                return FullNameParts[0];

            var email = Email;
            if (!string.IsNullOrWhiteSpace(email) && email.Contains('@'))
                return email.Split('@')[0];

            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the last name of the user from the surname claim.
    /// </summary>
    public string LastName
    {
        get
        {
            var lastName = ClaimValue(
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname",
                "family_name");
            if (!string.IsNullOrWhiteSpace(lastName))
                return lastName;

            if (FullNameParts.Length > 1)
                return string.Join(" ", FullNameParts.Skip(1));

            return FirstName;
        }
    }

    /// <summary>
    /// Gets the email address from the User Principal Name (UPN) claim.
    /// </summary>
    public string Email => ClaimValue(
        "email",
        "preferred_username",
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn",
        "upn");

    /// <summary>
    /// Gets the collection of OAuth scopes granted for the current request.
    /// </summary>
    /// <remarks>Scopes are typically used to define the permissions or access levels granted to the application. 
    /// This property retrieves the scopes from the user's claims, specifically from the claim with the type 
    /// "http://schemas.microsoft.com/identity/claims/scope".</remarks>
    public ICollection<string> Scopes => context?.User.FindFirst("http://schemas.microsoft.com/identity/claims/scope")?.Value?.Split(' ').ToList() ?? [];

    /// <summary>
    /// Gets the collection of roles assigned to the authenticated user.
    /// </summary>
    public ICollection<string> Roles => context?.User.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Select(c => c.Value).ToList() ?? [];

    /// <summary>
    /// Gets the collection of security group identifiers from the groups claim.
    /// </summary>
    public ICollection<string> Groups => context?.User.FindAll("groups").Select(c => c.Value).ToList() ?? [];
}