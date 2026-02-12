namespace Goodtocode.AgentFramework.Core.Domain.Auth;

public struct UserRoles
{
    public const string ChatOwner = "AssetOwner";
    public const string ChatEditor = "AssetEditor";
    public const string ChatViewer = "AssetViewer";
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
    public bool CanView => Roles.Contains(UserRoles.ChatOwner) || Roles.Contains(UserRoles.ChatEditor) || Roles.Contains(UserRoles.ChatViewer);
    public bool CanEdit => Roles.Contains(UserRoles.ChatOwner) || Roles.Contains(UserRoles.ChatEditor);
    public bool CanDelete => Roles.Contains(UserRoles.ChatOwner);

    public static UserContext Create(Guid ownerId, Guid tenantId, string firstName, string lastName, string email, IEnumerable<string> roles)
    {
        return new UserContext
        {
            OwnerId = ownerId,
            TenantId = tenantId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Roles = roles
        };
    }

}