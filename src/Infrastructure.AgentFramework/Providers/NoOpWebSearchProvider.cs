using Microsoft.Extensions.Logging;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Providers;

public sealed class NoOpWebSearchProvider(ILogger<NoOpWebSearchProvider> logger) : IWebSearchProvider
{
    private readonly ILogger<NoOpWebSearchProvider> _logger = logger;

    public Task<WebSearchResult> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(query))
        {
            _logger.LogInformation("Web search is disabled or not configured. Returning no results for query '{Query}'.", query);
        }

        return Task.FromResult(new WebSearchResult
        {
            Query = query,
            Results = []
        });
    }
}
