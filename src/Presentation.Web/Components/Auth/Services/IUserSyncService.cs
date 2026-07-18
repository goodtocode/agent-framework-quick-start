using System.Security.Claims;

namespace Goodtocode.AgentFramework.Presentation.Web.Components.Auth.Services;

public interface IUserSyncService
{
    void UserChanged(ClaimsPrincipal? user);
    Task SyncUserAsync(ClaimsPrincipal? user);
}
