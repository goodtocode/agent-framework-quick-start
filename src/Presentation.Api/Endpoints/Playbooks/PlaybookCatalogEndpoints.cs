using Goodtocode.AgentFramework.Core.Application.Playbooks;
using Goodtocode.AgentFramework.Core.Domain.Playbooks;

namespace Goodtocode.AgentFramework.Presentation.Api.Endpoints.Playbooks;

/// <summary>
/// Maps the Playbook catalog CRUD endpoints: query, create, update, and delete a Playbook and its
/// CER step content. Playbooks are shared reference data (the example catalog this template
/// ships with), so this group is not owner/tenant scoped the way <c>my</c>/<c>our</c> endpoints
/// are - only the separate "run" endpoints in <see cref="MyPlaybookEndpoints"/> are.
/// </summary>
public static class PlaybookCatalogEndpoints
{
    public static IEndpointRouteBuilder MapPlaybookCatalogEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints
            .MapGroup("api/v{version:apiVersion}/playbooks")
            .RequireAuthorization();
        group.WithApiVersionSet(versionSet).HasApiVersion(new ApiVersion(1, 0));

        group.MapGet("", GetPlaybooks)
            .WithName("GetPlaybooks")
            .Produces<ICollection<PlaybookDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("{id:guid}", GetPlaybookById)
            .WithName("GetPlaybookById")
            .Produces<PlaybookDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("by-key/{key}", GetPlaybookByKey)
            .WithName("GetPlaybookByKey")
            .Produces<PlaybookDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", CreatePlaybook)
            .WithName("CreatePlaybook")
            .Produces<PlaybookDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPut("{id:guid}", UpdatePlaybook)
            .WithName("UpdatePlaybook")
            .Produces<PlaybookDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("{id:guid}/steps/{stepType}", UpdatePlaybookStep)
            .WithName("UpdatePlaybookStep")
            .Produces<PlaybookDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("{id:guid}", DeletePlaybook)
            .WithName("DeletePlaybook")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> GetPlaybooks(ISender sender)
    {
        var playbooks = await sender.Send(new GetPlaybooksQuery());
        return ApiResponseMapper.ListOrOk(playbooks);
    }

    private static async Task<IResult> GetPlaybookById(ISender sender, Guid id)
    {
        var playbook = await sender.Send(new GetPlaybookQuery { Id = id });
        return ApiResponseMapper.SingleOrNotFound(playbook);
    }

    private static async Task<IResult> GetPlaybookByKey(ISender sender, string key)
    {
        var playbook = await sender.Send(new GetPlaybookQuery { Key = key });
        return ApiResponseMapper.SingleOrNotFound(playbook);
    }

    private static async Task<IResult> CreatePlaybook(HttpContext httpContext, ISender sender, CreatePlaybookCommand command)
    {
        var response = await sender.Send(command);
        var version = httpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";

        return TypedResults.CreatedAtRoute(response, "GetPlaybookById", new { version, id = response.Id });
    }

    private static async Task<IResult> UpdatePlaybook(ISender sender, Guid id, UpdatePlaybookBody body)
    {
        var response = await sender.Send(new UpdatePlaybookCommand { Id = id, Name = body.Name, Description = body.Description });
        return TypedResults.Ok(response);
    }

    private static async Task<IResult> UpdatePlaybookStep(ISender sender, Guid id, PlaybookStepType stepType, UpdatePlaybookStepBody body)
    {
        var response = await sender.Send(new UpdatePlaybookStepCommand
        {
            PlaybookId = id,
            StepType = stepType,
            Name = body.Name,
            Description = body.Description,
            ActionFormat = body.ActionFormat,
            ActionDefinition = body.ActionDefinition
        });
        return TypedResults.Ok(response);
    }

    private static async Task<IResult> DeletePlaybook(ISender sender, Guid id)
    {
        var result = await sender.Send(new DeletePlaybookCommand { Id = id });
        return result.IsSuccess ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    public sealed class UpdatePlaybookBody
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
    }

    public sealed class UpdatePlaybookStepBody
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
        public PlaybookActionFormat? ActionFormat { get; init; }
        public string? ActionDefinition { get; init; }
    }
}
