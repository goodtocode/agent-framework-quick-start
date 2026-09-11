namespace Goodtocode.AgentFramework.Presentation.Web.Features.Chats.Models;

public sealed class ChatSelectionOption
{
    public string Kind { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;

    public string BuildSelectionPrompt()
    {
        return Kind.ToLowerInvariant() switch
        {
            "actor" => $"Select actor {Id}",
            "chatsession" => $"Select chat session {Id}",
            _ => $"Select {Kind} {Id}"
        };
    }
}
