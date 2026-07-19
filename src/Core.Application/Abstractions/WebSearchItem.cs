namespace Goodtocode.AgentFramework.Core.Application.Abstractions;

public sealed class WebSearchItem
{
    public required string Title { get; init; }

    public required string Url { get; init; }

    public string? Snippet { get; init; }

    public string? Source { get; init; }
}
