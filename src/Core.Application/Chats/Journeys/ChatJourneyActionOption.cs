namespace Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

/// <summary>
/// A clickable follow-up selection presented after a journey step, parsed from the selection
/// tokens emitted by the routed tool response. Clicking an action submits
/// <see cref="BuildSelectionPrompt"/> through the normal message-input path.
/// </summary>
public sealed record ChatJourneyActionOption(string Kind, string Id, string Value, string Label)
{
    public string BuildSelectionPrompt()
        => Kind.ToLowerInvariant() switch
        {
            "actor" => $"Select actor {Id}",
            "chatsession" => $"Select chat session {Id}",
            _ => $"Select {Kind} {Id}"
        };
}
