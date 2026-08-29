using Goodtocode.Mediator;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Execution;

/// <summary>
/// Executes application requests for AI tools through the mediator pipeline.
/// </summary>
public interface IToolApplicationExecutor
{
    /// <summary>
    /// Sends a request with a response through the application mediator pipeline.
    /// </summary>
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a request without a response through the application mediator pipeline.
    /// </summary>
    Task SendAsync(IRequest request, CancellationToken cancellationToken);
}