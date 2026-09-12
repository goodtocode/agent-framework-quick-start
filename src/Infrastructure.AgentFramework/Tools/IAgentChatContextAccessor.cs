namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

/// <summary>
/// Tracks per-chat-session selection state (currently selected actor, currently selected chat
/// session under that actor) so deterministic intents can answer follow-up/parameter questions
/// like "query chat sessions for the selected actor" without the user repeating an id.
/// </summary>
public interface IAgentChatContextAccessor
{
    Guid? CurrentChatSessionId { get; }

    void SetCurrentChatSession(Guid chatSessionId);

    void ClearCurrentChatSession();

    ChatToolSessionContext GetOrCreateContext(Guid chatSessionId);

    void UpsertContext(Guid chatSessionId, Func<ChatToolSessionContext, ChatToolSessionContext> update);
}
