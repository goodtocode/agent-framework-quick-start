using System.ComponentModel;
using Goodtocode.AgentFramework.Core.Application.Actors;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

public class ActorResponse : IActorResponse
{
    public Guid ActorId { get; set; }
    public string? Name { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
}


public sealed class ActorsTool(IServiceProvider serviceProvider) : ScopedAgentTool(serviceProvider), IActorsTool
{
    public static string ToolName => "ActorsTool";
    public string FunctionName => _currentFunctionName;
    public Dictionary<string, object> Parameters => _currentParameters;

    private string _currentFunctionName = string.Empty;
    private Dictionary<string, object> _currentParameters = [];

    [Description(
        """
        Lists all actor records in the current tenant.

        Use this read-only tool whenever the user asks to list, show, or browse actors. Call it
        immediately without asking permission, announcing the call, or searching the web. Return
        only the actor records returned by the application query; never invent actor IDs.
        """)]
    public async Task<ICollection<IActorResponse>> GetActorsAsync(CancellationToken cancellationToken)
    {
        _currentFunctionName = "get_actors";
        _currentParameters = [];

        var actors = await SendAsync(new GetOurActorsQuery(), cancellationToken);
        return [.. actors.Select(CreateResponse)];
    }

    [Description("Lists actor records owned by the current authenticated user. This is a read-only query; call it immediately without confirmation, announcements, or web search.")]
    public async Task<ICollection<IActorResponse>> GetMyActorsAsync(CancellationToken cancellationToken)
    {
        _currentFunctionName = "get_my_actors";
        _currentParameters = [];

        var actors = await SendAsync(new GetMyActorsQuery(), cancellationToken);
        return [.. actors.Select(CreateResponse)];
    }

    [Description(
        """
        Looks up a single actor (user/profile record) by their actorId (a GUID).

        For a request to list actors without a name, use GetActorsAsync immediately. This is a
        read-only query and does not require confirmation or web search.

        Use this tool whenever the user asks things like:
        - get actor {id}
        - look up actor with id {id}
        - find the actor whose id is {id}
        - what is the status of actor {id}

        Always call this tool for these requests instead of answering from memory, claiming you
        lack access, or asking permission first. Returns a structured status (Found, Partial,
        NotFound) with a human-readable message.
        """)]
    public async Task<IActorResponse?> GetActorByIdAsync(Guid actorId, CancellationToken cancellationToken)
    {
        _currentFunctionName = "get_actor_by_id";
        _currentParameters = new()
        {
            { "actorId", actorId }
        };

        var actor = await SendAsync(new GetOurActorQuery
        {
            ActorId = actorId
        }, cancellationToken);

        if (actor == null)
        {
            return null;
        }

        return new ActorResponse
        {
            ActorId = actorId,
            Name = $"{actor.FirstName} {actor.LastName}",
            Status = string.IsNullOrWhiteSpace($"{actor.FirstName} {actor.LastName}") ? "Partial" : "Found",
            Message = string.IsNullOrWhiteSpace($"{actor.FirstName} {actor.LastName}")
                ? "Actor exists but name is not yet linked to Entra External ID."
                : "Actor found."
        };
    }

    [Description(
        """
        Searches actors (user/profile records) by name (full or partial match) in the current tenant.

        Use this tool whenever the user asks things like:
        - find actor named {name}
        - search actors for {name}
        - who is {name}
        - look up a user called {name}

        Always call this tool for these requests instead of answering from memory, claiming you
        lack access, or asking permission first. Returns a collection of structured matches with
        actorId, name, status, and message (or a single NotFound entry if nothing matches). Never
        use this to search other tenants.
        """)]
    public async Task<ICollection<IActorResponse>> GetActorsByNameAsync(string name, CancellationToken cancellationToken)
    {
        _currentFunctionName = "get_actors_by_name";
        _currentParameters = new()
        {
            { "name", name }
        };

        var actors = await SendAsync(new GetOurActorsByNameQuery
        {
            Name = name
        }, cancellationToken);

        if (actors.Count == 0)
        {
            return [new ActorResponse
            {
                ActorId = Guid.Empty,
                Name = name,
                Status = "NotFound",
                Message = "No actor found with the specified name."
            }];
        }

        return [.. actors.Select(CreateResponse)];
    }

    private static ActorResponse CreateResponse(ActorDto actor)
    {
        var name = $"{actor.FirstName} {actor.LastName}";
        return new ActorResponse
        {
            ActorId = actor.Id,
            Name = name,
            Status = string.IsNullOrWhiteSpace(name) ? "Partial" : "Found",
            Message = string.IsNullOrWhiteSpace(name)
                ? "Actor exists but name is not yet linked to Entra External ID."
                : "Actor found."
        };
    }
}
