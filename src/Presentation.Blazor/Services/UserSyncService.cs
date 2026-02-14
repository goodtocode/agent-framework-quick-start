using Goodtocode.AgentFramework.Presentation.WebApi.Client;
using System.Security.Authentication;
using System.Security.Claims;
using Goodtocode.AgentFramework.Presentation.Blazor.Components.Auth.Services;
using Goodtocode.AgentFramework.Presentation.Blazor.Components.Auth;

namespace Goodtocode.AgentFramework.Presentation.Blazor.Services;

public class UserSyncService(BackendApiClient apiClient, IUserClaimsInfo userInfo) : ApiService, IUserSyncService
{
    private readonly BackendApiClient _apiClient = apiClient;
    private readonly IUserClaimsInfo _userContext =  userInfo;

    public const string UserSyncClaimName = "user_sync_status";

    public enum SyncStatus
    {
        Pending,
        Synced,
        Failed
    }

    private bool _isSyncing;

    public void UserChanged(ClaimsPrincipal? user)
    {
        SetSyncStatus(user, SyncStatus.Pending);
    }

    public async Task SyncUserAsync(ClaimsPrincipal? user)
    {
        if (_isSyncing) return;
        _isSyncing = true;
        try 
        {
            ClaimsIdentity? identity = user?.Identity as ClaimsIdentity ?? throw new AuthenticationException("User identity is missing or invalid.");

            var syncClaim = identity.FindFirst(UserSyncClaimName)?.Value ?? UserSyncService.SyncStatus.Pending.ToString();
            if (identity.IsAuthenticated && syncClaim == UserSyncService.SyncStatus.Pending.ToString())
            {
                await HandleApiException(() => _apiClient.SaveMyActorAsync(new SaveMyActorCommand
                {
                    FirstName = _userContext.Givenname,
                    LastName = _userContext.Surname,
                    Email = _userContext.Email
                }));
            }
            SetSyncStatus(user, SyncStatus.Synced);
        }
        catch (Exception)
        {
            SetSyncStatus(user, SyncStatus.Failed);
            throw;
        }
        finally
        {
            _isSyncing = false;
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
