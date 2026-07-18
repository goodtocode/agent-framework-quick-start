namespace Goodtocode.AgentFramework.Core.Application.Common.Auth;

/// <summary>
/// Represents the authenticated user's context, including their identity, tenant association, and authorization permissions.
/// </summary>
/// <remarks>This interface provides access to user identity information and computed authorization permissions
/// based on assigned roles. It serves as the domain representation of the current user's security context.</remarks>
public interface IUserContext : IRlsContext
{
    string FirstName { get; }
    string LastName { get; }
    string Email { get; }
    IEnumerable<string> Roles { get; }
    bool CanView { get; }
    bool CanEdit { get; }
    bool CanDelete { get; }
}
