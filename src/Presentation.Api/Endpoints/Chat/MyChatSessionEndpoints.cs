using Goodtocode.AgentFramework.Core.Application.Chat;

namespace Goodtocode.AgentFramework.Presentation.Api.Endpoints.Chat;

/// <summary>
/// Maps chat session endpoints.
/// </summary>
public static class MyChatSessionEndpoints
{
    /// <summary>
    /// Registers chat session endpoint mappings.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="versionSet">The API version set that declares supported versions for this endpoint group, enabling Asp.Versioning to substitute the version token in the route template (e.g. <c>api/v{version:apiVersion}</c> → <c>api/v1</c>) and generate correct OpenAPI paths.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapMyChatSessionEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints
            .MapGroup("api/v{version:apiVersion}/my/chat")
            .RequireAuthorization();
        group.WithApiVersionSet(versionSet).HasApiVersion(new ApiVersion(1, 0));

        group.MapGet("ChatSessions", GetAll)
            .WithName("GetMyChatSessions")
            .Produces<ICollection<ChatSessionDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("Paginated", GetPaginated)
            .WithName("GetMyChatSessionsPaginated")
            .Produces<PaginatedList<ChatSessionDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("{id:guid}", Get)
            .WithName("GetMyChatSession")
            .Produces<ChatSessionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("", Post)
            .WithName("CreateMyChatSession")
            .Produces<ChatSessionDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);

        return endpoints;
    }

    private static async Task<ICollection<ChatSessionDto>> GetAll(ISender sender)
    {
        return await sender.Send(new GetMyChatSessionsQuery());
    }

    private static async Task<IResult> GetPaginated(
        ISender sender,
        DateTime? startDate,
        DateTime? endDate,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = new GetMyChatSessionsPaginatedQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    private static async Task<ChatSessionDto> Get(ISender sender, Guid id)
    {
        return await sender.Send(new GetMyChatSessionQuery { Id = id });
    }

    private static async Task<IResult> Post(HttpContext httpContext, ISender sender, CreateMyChatSessionCommand command)
    {
        var response = await sender.Send(command);
        var version = httpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";

        return TypedResults.CreatedAtRoute(
            response,
            "GetMyChatSession",
            new { version, id = response.Id });
    }
}
