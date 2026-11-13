using System.Security.Claims;

namespace Goodtocode.AgentStart.Presentation.Web.Library.Auth.Services;

public interface IUserSyncService
{
    void UserChanged(ClaimsPrincipal? user);
    Task SyncUserAsync(ClaimsPrincipal? user);
}
