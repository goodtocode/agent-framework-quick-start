using Goodtocode.AgentFramework.Core.Application.Chat;

namespace Goodtocode.AgentFramework.Presentation.Api.Endpoints.Chat;

/// <summary>
/// Maps chat message endpoints.
/// </summary>
public static class MyChatMessageEndpoints
{
    /// <summary>
    /// Registers chat message endpoint mappings.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="versionSet">The API version set that declares supported versions for this endpoint group, enabling Asp.Versioning to substitute the version token in the route template (e.g. <c>api/v{version:apiVersion}</c> → <c>api/v1</c>) and generate correct OpenAPI paths.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapMyChatMessageEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints
            .MapGroup("api/v{version:apiVersion}/my/messages")
            .RequireAuthorization();
        group.WithApiVersionSet(versionSet).HasApiVersion(new ApiVersion(1, 0));

        group.MapGet("{id:guid}", Get)
            .WithName("GetMyChatMessage")
            .Produces<ChatMessageDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("Paginated", GetPaginated)
            .WithName("GetMyChatMessagesPaginated")
            .Produces<PaginatedList<ChatMessageDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("", Post)
            .WithName("CreateMyChatMessage")
            .Produces<ChatMessageDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPatch("{id:guid}", Patch)
            .WithName("PatchMyChatSession")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        return endpoints;
    }

    private static async Task<ChatMessageDto> Get(ISender sender, Guid id)
    {
        return await sender.Send(new GetMyChatMessageQuery { Id = id });
    }

    private static async Task<IResult> GetPaginated(ISender sender, [AsParameters] GetMyChatMessagesPaginatedQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> Post(HttpContext httpContext, ISender sender, CreateMyChatMessageCommand command)
    {
        var response = await sender.Send(command);
        var version = httpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";

        return TypedResults.CreatedAtRoute(
            response,
            "GetMyChatMessage",
            new { version, id = response.Id });
    }

    private static async Task<IResult> Patch(ISender sender, Guid id, PatchMyChatSessionCommand command)
    {
        command.Id = id;
        await sender.Send(command);
        return TypedResults.NoContent();
    }
}
