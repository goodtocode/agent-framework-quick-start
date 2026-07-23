using Goodtocode.SecuredHttpClient.Middleware;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Options;
using Goodtocode.AgentFramework.Presentation.Web.Infrastructure.Options;

namespace Goodtocode.AgentFramework.Presentation.Web.Library.Auth.Middleware;

public class DownstreamApiAccessTokenProvider(IHttpContextAccessor httpContextAccessor, ITokenAcquisition tokenAcquisition, IOptions<BackendApiOptions> backendApiOptions) : IAccessTokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ITokenAcquisition _tokenAcquisition = tokenAcquisition;
    private readonly IOptions<BackendApiOptions> _backendApiOptions = backendApiOptions;

    public async Task<string> GetAccessTokenAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (!context?.User?.Identity?.IsAuthenticated == true)
            return string.Empty;

        var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync([
            $"api://{_backendApiOptions.Value.ClientId}/access_as_user"
        ], user: context?.User);

        return accessToken ?? string.Empty;
    }
}