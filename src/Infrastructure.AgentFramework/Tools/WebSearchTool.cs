using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

public sealed class WebSearchTool(IServiceProvider serviceProvider) : ScopedAgentTool(serviceProvider)
{
    [Description(
        """
        Searches the public web for current external information and returns ranked results.

        Use this tool for requests such as "search the web for {query}", current news, or current
        public documentation when the answer is not available from the user's chat sessions or
        actor data. Always call this tool for those requests instead of answering from memory,
        claiming you lack access, asking permission, or promising to search later.
        """)]
    public async Task<WebSearchResult> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        return await ResolveScopedAsync(async provider =>
        {
            var webSearchProvider = provider.GetRequiredService<IWebSearchProvider>();
            return await webSearchProvider.SearchAsync(query, cancellationToken);
        });
    }
}