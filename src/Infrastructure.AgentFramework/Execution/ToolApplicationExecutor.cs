using Goodtocode.Mediator;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Execution;

/// <summary>
/// Default mediator-backed executor used by AI tools for application commands and queries.
/// </summary>
public sealed class ToolApplicationExecutor(ISender sender) : IToolApplicationExecutor
{
    private readonly ISender _sender = sender;

    /// <inheritdoc />
    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        => _sender.Send(request, cancellationToken);

    /// <inheritdoc />
    public Task SendAsync(IRequest request, CancellationToken cancellationToken)
        => _sender.Send(request, cancellationToken);
}