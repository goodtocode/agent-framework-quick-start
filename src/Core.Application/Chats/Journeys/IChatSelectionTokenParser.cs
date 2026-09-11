namespace Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

/// <summary>
/// Parses the <c>[selection|kind|id|value|label]</c> tokens a routed tool response emits into
/// follow-up action options, and strips them from display content.
/// </summary>
public interface IChatSelectionTokenParser
{
    IReadOnlyList<ChatJourneyActionOption> Parse(string? content);

    string Strip(string? content);
}
