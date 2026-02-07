namespace Goodtocode.AgentFramework.Core.Application.Abstractions;

public interface IActorsTool
{
    Task<IActorResponse> GetActorByIdAsync(Guid actorId, CancellationToken cancellationToken);
    Task<ICollection<IActorResponse>> GetActorsByNameAsync(string name, CancellationToken cancellationToken);
}