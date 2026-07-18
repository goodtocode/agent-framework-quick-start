using Goodtocode.AgentFramework.Core.Application.Actor;

namespace Goodtocode.AgentFramework.Presentation.Api.Endpoints.Actor;

/// <summary>
/// Maps identity actor endpoints.
/// </summary>
public static class MyActorEndpoints
{
    /// <summary>
    /// Registers actor-related minimal API endpoints.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="versionSet">The API version set that declares supported versions for this endpoint group, enabling Asp.Versioning to substitute the version token in the route template (e.g. <c>api/v{version:apiVersion}</c> → <c>api/v1</c>) and generate correct OpenAPI paths.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapMyActorEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints
            .MapGroup("api/v{version:apiVersion}/my/actors")
            .RequireAuthorization();
        group.WithApiVersionSet(versionSet).HasApiVersion(new ApiVersion(1, 0));

        group.MapGet("{ownerId:guid}", GetMyActorProfile)
            .WithName("GetMyActorProfile")
            .Produces<ActorDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPut("", SaveMyActor)
            .WithName("SaveMyActor")
            .Produces<ActorDto>(StatusCodes.Status200OK)
            .Produces<ActorDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);

        return endpoints;
    }

    private static async Task<ActorDto> GetMyActorProfile(ISender sender, Guid ownerId)
    {
        return await sender.Send(new GetMyActorQuery { OwnerId = ownerId });
    }

    private static async Task<IResult> SaveMyActor(HttpContext httpContext, ISender sender, SaveMyActorCommand command)
    {
        var response = await sender.Send(command);
        var version = httpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";

        return TypedResults.CreatedAtRoute(
            response,
            "GetMyActorProfile",
            new { version, ownerId = response.OwnerId });
    }
}
