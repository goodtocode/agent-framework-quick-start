namespace Goodtocode.AgentStart.Core.Application.Abstractions;

public interface IWebSearchProvider
{
    Task<WebSearchResult> SearchAsync(
        string query,
        CancellationToken cancellationToken = default);
}
