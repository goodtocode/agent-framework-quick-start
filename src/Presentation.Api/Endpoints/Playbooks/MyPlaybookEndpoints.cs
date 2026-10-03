using Goodtocode.AgentFramework.Core.Application.Playbooks;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Essay;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Persistence;
using Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Taxonomy;

namespace Goodtocode.AgentFramework.Presentation.Api.Endpoints.Playbooks;

/// <summary>
/// Maps the three example Playbook "run" endpoints (SQL Statistics, Taxonomy, Essay). Each takes
/// the plain-string Collect-stage input and returns the shared <see cref="PlaybookExecutionResultDto"/>
/// shape, so Presentation.Web can render all three playbooks with one component set. See
/// <c>docs/governance/playbook-workflow-types.md</c> for the unified Collect/Evaluate/Record
/// convention these endpoints expose.
/// </summary>
public static class MyPlaybookEndpoints
{
    /// <summary>
    /// Registers Playbook endpoint mappings.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="versionSet">The API version set that declares supported versions for this endpoint group, enabling Asp.Versioning to substitute the version token in the route template (e.g. <c>api/v{version:apiVersion}</c> → <c>api/v1</c>) and generate correct OpenAPI paths.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapMyPlaybookEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints
            .MapGroup("api/v{version:apiVersion}/my/playbooks")
            .RequireAuthorization();
        group.WithApiVersionSet(versionSet).HasApiVersion(new ApiVersion(1, 0));

        group.MapPost("sql-statistics", RunSqlStatistics)
            .WithName("RunMySqlStatisticsPlaybook")
            .Produces<PlaybookExecutionResultDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("taxonomy", RunTaxonomy)
            .WithName("RunMyTaxonomyPlaybook")
            .Produces<PlaybookExecutionResultDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("essay", RunEssay)
            .WithName("RunMyEssayPlaybook")
            .Produces<PlaybookExecutionResultDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("{playbookKey}/latest-execution", GetLatestExecution)
            .WithName("GetMyLatestPlaybookExecution")
            .Produces<PlaybookExecutionResultDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> RunSqlStatistics(ISender sender, RunSqlStatisticsPlaybookCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> RunTaxonomy(ISender sender, RunTaxonomyPlaybookCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> RunEssay(ISender sender, RunEssayPlaybookCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetLatestExecution(ISender sender, string playbookKey)
    {
        var result = await sender.Send(new GetMyLatestPlaybookExecutionQuery { PlaybookKey = playbookKey });
        return ApiResponseMapper.SingleOrNotFound(result);
    }
}
