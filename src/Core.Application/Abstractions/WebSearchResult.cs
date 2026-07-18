namespace Goodtocode.AgentFramework.Core.Application.Abstractions;

public sealed class WebSearchResult
{
    public required string Query { get; init; }

    public List<WebSearchItem> Results { get; init; } = [];
}
