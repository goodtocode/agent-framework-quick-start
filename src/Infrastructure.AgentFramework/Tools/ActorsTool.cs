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
        Looks up a single actor (user/profile record) by their actorId (a GUID).

        Use this tool whenever the user asks things like:
        - get actor {id}
        - look up actor with id {id}
        - find the actor whose id is {id}
        - what is the status of actor {id}

        Returns a structured status (Found, Partial, NotFound) with a human-readable message.
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

        Returns a collection of structured matches with actorId, name, status, and message (or a
        single NotFound entry if nothing matches). Never use this to search other tenants.
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

        return [.. actors.Select(a => new ActorResponse
        {
            ActorId = a.Id,
            Name = $"{a.FirstName} {a.LastName}",
            Status = string.IsNullOrWhiteSpace($"{a.FirstName} {a.LastName}") ? "Partial" : "Found",
            Message = string.IsNullOrWhiteSpace($"{a.FirstName} {a.LastName}")
                ? "Actor exists but name is not yet linked to Entra External ID."
                : "Actor found."
        })];
    }
}
