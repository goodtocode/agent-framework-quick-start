using Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Journeys;

/// <summary>
/// The built-in chat journey: top-level prompts start a journey at an aggregate, and each
/// selection level narrows the follow-up prompts. Every prompt here is phrased to match a
/// deterministic intent in <c>DefaultIntentCatalogFactory</c> so Tier 1 routing wins before the
/// message ever reaches the LLM.
/// </summary>
public sealed class DefaultChatJourneyCatalogContributor : IChatJourneyCatalogContributor
{
    public IReadOnlyList<ChatJourneyDefinition> GetDefinitions() =>
    [
        new ChatJourneyDefinition(ChatJourneyLevel.None,
        [
            "List my chat sessions",
            "List my messages for this chat session",
            "Find an actor by name",
            "Search the web for current information"
        ]),

        new ChatJourneyDefinition(ChatJourneyLevel.ActorSelected,
        [
            "Query chat sessions for the selected actor",
            "Query my actor user profile"
        ]),

        // Mid-chain entry: the customer listed their own chat sessions, so the actor was implied by
        // their OwnerId rather than selected. Offer the same next step as the actor-first path.
        new ChatJourneyDefinition(ChatJourneyLevel.MyChatSessionsListed,
        [
            "List my messages for this chat session",
            "Tell me what type of chat sessions I have been having with the ai agent",
            "Query my actor user profile"
        ]),

        new ChatJourneyDefinition(ChatJourneyLevel.ChatSessionSelected,
        [
            "List my messages for this chat session",
            "Query chat messages for the selected chat session",
            "List my chat sessions"
        ])
    ];
}
