namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

/// <summary>
/// Per-chat-session state for deterministic follow-up/parameter questions (e.g. "select actor
/// {id}" then "query chat sessions for the selected actor"). Populated and read by
/// <see cref="ChatMessageIntentRouter"/> and tools via <see cref="IAgentChatContextAccessor"/>.
/// </summary>
public sealed record ChatToolSessionContext
{
    public Guid? ActorId { get; init; }

    public Guid? SelectedChatSessionId { get; init; }

    /// <summary>
    /// Set when the customer entered the journey mid-chain by listing their own chat sessions
    /// (scoped by the authenticated user's OwnerId) rather than by selecting an actor first.
    /// </summary>
    public bool MyChatSessionsListed { get; init; }
}
