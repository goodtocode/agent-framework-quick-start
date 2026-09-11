namespace Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

/// <summary>
/// The accumulated selections a customer has made during a chat journey. The deepest populated
/// selection determines the <see cref="ChatJourneyLevel"/> used to resolve the next set of
/// suggested prompts.
/// </summary>
public sealed record ChatJourneyContext
{
    public Guid? ActorId { get; init; }

    public Guid? SelectedChatSessionId { get; init; }

    /// <summary>
    /// Set when the customer entered the journey mid-chain by listing their own chat sessions,
    /// scoped by the authenticated user's OwnerId rather than by an explicit actor selection.
    /// </summary>
    public bool MyChatSessionsListed { get; init; }

    /// <summary>
    /// Optional scope discriminator allowing a specialized catalog entry to win over the global
    /// entry for the same level (for example a domain-specific chat experience).
    /// </summary>
    public string? ScopeCode { get; init; }

    public ChatJourneyLevel ResolveLevel()
    {
        if (SelectedChatSessionId.HasValue)
        {
            return ChatJourneyLevel.ChatSessionSelected;
        }

        if (MyChatSessionsListed)
        {
            return ChatJourneyLevel.MyChatSessionsListed;
        }

        return ActorId.HasValue ? ChatJourneyLevel.ActorSelected : ChatJourneyLevel.None;
    }
}
