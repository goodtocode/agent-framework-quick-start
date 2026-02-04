namespace Goodtocode.AgentFramework.Core.Application.Abstractions;

public interface IActorsPlugin
{
    Task<IActorResponse> GetActorByIdAsync(Guid actorId, CancellationToken cancellationToken);
    Task<ICollection<IActorResponse>> GetActorsByNameAsync(string name, CancellationToken cancellationToken);
}