namespace Goodtocode.AgentFramework.Core.Application.Common.Auth;

public struct UserRoles
{
    public const string Owner = "AssetOwner";
    public const string Editor = "AssetEditor";
    public const string Viewer = "AssetViewer";
}

/// <summary>
/// Represents the authenticated user's context in the domain, including identity and authorization state.
/// </summary>
/// <remarks>This class encapsulates user identity information and provides role-based authorization logic
/// through computed properties (CanView, CanEdit, CanDelete).</remarks>
public class UserContext() : IUserContext
{
    public Guid OwnerId { get; private set; }
    public Guid TenantId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public IEnumerable<string> Roles { get; private set; } = [];
    public bool CanView => Roles.Contains(UserRoles.Owner) || Roles.Contains(UserRoles.Editor) || Roles.Contains(UserRoles.Viewer);
    public bool CanEdit => Roles.Contains(UserRoles.Owner) || Roles.Contains(UserRoles.Editor);
    public bool CanDelete => Roles.Contains(UserRoles.Owner);

    public static UserContext Create(IClaimsReader claimsReader)
    {
        return new UserContext
        {
            OwnerId = claimsReader.ObjectId,
            TenantId = claimsReader.TenantId,
            FirstName = claimsReader.FirstName,
            LastName = claimsReader.LastName,
            Email = claimsReader.Email,
            Roles = claimsReader.Roles
        };
    }

}