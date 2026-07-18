using System.ComponentModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

public sealed class WebSearchTool(IServiceProvider serviceProvider) : AITool
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    [Description("Search the public web and return ranked results.")]
    public async Task<WebSearchResult> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var webSearchProvider = scope.ServiceProvider.GetRequiredService<IWebSearchProvider>();
        return await webSearchProvider.SearchAsync(query, cancellationToken);
    }
}