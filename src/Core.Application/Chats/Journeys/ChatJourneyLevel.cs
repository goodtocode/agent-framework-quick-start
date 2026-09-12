namespace Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

/// <summary>
/// The depth a customer has reached within a chat journey. A journey starts at
/// <see cref="None"/> (top-level aggregate prompts) and deepens as the customer selects an
/// aggregate instance, which narrows the follow-up prompts and actions that are offered.
/// A journey can also be entered mid-chain: listing the current user's own chat sessions reaches
/// <see cref="MyChatSessionsListed"/> directly, because the actor is implied by the authenticated
/// user's OwnerId rather than chosen.
/// </summary>
public enum ChatJourneyLevel
{
    None = 0,
    ActorSelected = 1,
    MyChatSessionsListed = 2,
    ChatSessionSelected = 3
}
