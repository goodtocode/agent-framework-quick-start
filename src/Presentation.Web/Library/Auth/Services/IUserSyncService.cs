using System.Security.Claims;

namespace Goodtocode.AgentFramework.Presentation.Web.Library.Auth.Services;

public interface IUserSyncService
{
    void UserChanged(ClaimsPrincipal? user);
    Task SyncUserAsync(ClaimsPrincipal? user);
}
