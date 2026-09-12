namespace Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

/// <summary>
/// The current position in a chat journey: the resolved level, the suggested prompts that can
/// start or continue the journey, and the follow-up action options that match the journey context.
/// </summary>
public sealed record ChatJourneyStep(
    ChatJourneyLevel Level,
    IReadOnlyList<ChatJourneySuggestedPrompt> SuggestedPrompts,
    IReadOnlyList<ChatJourneyActionOption> ActionOptions)
{
    public static ChatJourneyStep Empty { get; } = new(ChatJourneyLevel.None, [], []);
}
