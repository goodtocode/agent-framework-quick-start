using System.ComponentModel;
using Goodtocode.AgentFramework.Core.Application.Actor;

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

    [Description("Returns structured actor info by ID including name, status, and explanation.")]
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

    [Description("Returns structured actor info by name including ID, status, and explanation.")]
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
