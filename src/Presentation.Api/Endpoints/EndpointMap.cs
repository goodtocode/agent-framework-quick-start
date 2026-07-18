using Goodtocode.AgentFramework.Presentation.Api.Endpoints.Actor;
using Goodtocode.AgentFramework.Presentation.Api.Endpoints.Chat;

namespace Goodtocode.AgentFramework.Presentation.Api.Endpoints;

/// <summary>
/// Maps all minimal API endpoint groups for the Presentation API.
/// </summary>
public static class EndpointMap
{
    /// <summary>
    /// Registers all minimal API endpoint groups.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapMinimalApis(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        endpoints.MapMyChatMessageEndpoints(versionSet);
        endpoints.MapMyActorEndpoints(versionSet);
        endpoints.MapMyChatSessionEndpoints(versionSet);

        return endpoints;
    }
}
