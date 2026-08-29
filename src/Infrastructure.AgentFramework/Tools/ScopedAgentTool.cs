using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Execution;
using Goodtocode.Mediator;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

/// <summary>
/// Shared base class for AI tools that execute application requests in an isolated scope.
/// </summary>
public abstract class ScopedAgentTool(IServiceProvider serviceProvider) : AITool
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Sends an application request with a response through a new dependency-injection scope.
    /// </summary>
    protected async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var executor = scope.ServiceProvider.GetRequiredService<IToolApplicationExecutor>();
        return await executor.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Sends an application request without a response through a new dependency-injection scope.
    /// </summary>
    protected async Task SendAsync(IRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var executor = scope.ServiceProvider.GetRequiredService<IToolApplicationExecutor>();
        await executor.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Executes a scoped infrastructure operation that is not an application request.
    /// </summary>
    protected async Task<TResponse> ResolveScopedAsync<TResponse>(Func<IServiceProvider, Task<TResponse>> action)
    {
        using var scope = _serviceProvider.CreateScope();
        return await action(scope.ServiceProvider);
    }
}