using System.Security.Authentication;
using System.Security.Claims;
using Goodtocode.AgentFramework.Presentation.Web.Infrastructure.Http;
using Goodtocode.AgentFramework.Presentation.Web.Library.Auth;
using Goodtocode.AgentFramework.Presentation.Web.Library.Auth.Services;

namespace Goodtocode.AgentFramework.Presentation.Web.Infrastructure.Auth;

public class UserSyncService(BackendApiClient apiClient, IClaimsReader userInfo) : ApiService, IUserSyncService
{
    private readonly BackendApiClient _apiClient = apiClient;
    private readonly IClaimsReader _userContext = userInfo;

    public const string UserSyncClaimName = "user_sync_status";

    public enum SyncStatus
    {
        Pending,
        Synced,
        Failed
    }

    private readonly object _syncGate = new();
    private Task _syncTask = Task.CompletedTask;

    public void UserChanged(ClaimsPrincipal? user)
    {
        SetSyncStatus(user, SyncStatus.Pending);
    }

    public Task SyncUserAsync(ClaimsPrincipal? user)
    {
        ClaimsIdentity? identity = user?.Identity as ClaimsIdentity ?? throw new AuthenticationException("User identity is missing or invalid.");

        if (!identity.IsAuthenticated)
            return Task.CompletedTask;

        var syncClaim = identity.FindFirst(UserSyncClaimName)?.Value;
        if (syncClaim == SyncStatus.Synced.ToString())
            return Task.CompletedTask;

        lock (_syncGate)
        {
            if (!_syncTask.IsCompleted)
                return _syncTask;

            _syncTask = SyncUserInternalAsync(user);
            return _syncTask;
        }
    }

    private async Task SyncUserInternalAsync(ClaimsPrincipal? user)
    {
        try
        {
            await HandleApiException(() => _apiClient.SaveMyActorAsync(new SaveMyActorCommand
            {
                FirstName = _userContext.FirstName,
                LastName = _userContext.LastName,
                Email = _userContext.Email
            }));

            SetSyncStatus(user, SyncStatus.Synced);
        }
        catch (Exception)
        {
            SetSyncStatus(user, SyncStatus.Failed);
            throw;
        }
    }

    protected void SetSyncStatus(ClaimsPrincipal? user, SyncStatus newStatus)
    {
        ClaimsIdentity? identity = user?.Identity as ClaimsIdentity ?? throw new AuthenticationException("User identity is missing or invalid.");
        var existingClaim = identity.FindFirst(UserSyncClaimName);
        if (existingClaim != null)
        {
            identity.RemoveClaim(existingClaim);
        }

        identity.AddClaim(new Claim(UserSyncClaimName, newStatus.ToString()));
    }
}
