using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

public sealed class WebSearchTool(IServiceProvider serviceProvider) : ScopedAgentTool(serviceProvider)
{
    [Description("Search the public web for current external information and return ranked results. Use only when the answer is not available from the user's chat sessions or actor data.")]
    public async Task<WebSearchResult> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        return await ResolveScopedAsync(async provider =>
        {
            var webSearchProvider = provider.GetRequiredService<IWebSearchProvider>();
            return await webSearchProvider.SearchAsync(query, cancellationToken);
        });
    }
}