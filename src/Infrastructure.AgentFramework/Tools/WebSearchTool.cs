using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

public sealed class WebSearchTool(IServiceProvider serviceProvider) : ScopedAgentTool(serviceProvider)
{
    [Description("Search the public web and return ranked results.")]
    public async Task<WebSearchResult> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        return await ResolveScopedAsync(async provider =>
        {
            var webSearchProvider = provider.GetRequiredService<IWebSearchProvider>();
            return await webSearchProvider.SearchAsync(query, cancellationToken);
        });
    }
}