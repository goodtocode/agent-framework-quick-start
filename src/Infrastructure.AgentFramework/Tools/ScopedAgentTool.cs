using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Execution;
using Goodtocode.Mediator;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

/// <summary>
/// Shared base class for AI tools that execute application requests in an isolated scope.
/// </summary>
public abstract class ScopedAgentTool(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>Creates functions from explicitly described methods, retaining typed schemas and scoped execution.</summary>
    /// <returns>Functions with stable, tool-qualified names.</returns>
    /// <exception cref="InvalidOperationException">The tool has no explicitly described methods.</exception>
    public IReadOnlyList<AIFunction> CreateFunctions()
    {
        var toolType = GetType();
        var functions = toolType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(method => method.GetCustomAttribute<DescriptionAttribute>() is not null)
            .Select(method => AIFunctionFactory.Create(method, this, new AIFunctionFactoryOptions
            {
                Name = CreateFunctionName(toolType, method)
            }))
            .ToList();
        if (functions.Count == 0)
        {
            throw new InvalidOperationException($"Tool '{toolType.Name}' has no described functions.");
        }

        return functions;
    }

    private static string CreateFunctionName(Type toolType, MethodInfo method)
    {
        var name = $"{toolType.Name}_{method.Name}";
        if (name.Length <= 64)
        {
            return name;
        }

        // Preserve a readable prefix and a stable suffix to avoid truncation collisions.
        var suffix = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(name)))[..16];
        return $"{name[..47]}_{suffix}";
    }

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