namespace Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

/// <summary>
/// Reads the accumulated journey selections for a chat session. Implemented in
/// Infrastructure.AgentFramework over the same per-session tool context the chat router writes to,
/// so journey resolution never depends on infrastructure types.
/// </summary>
public interface IChatJourneyContextAccessor
{
    ChatJourneyContext GetContext(Guid chatSessionId);
}
