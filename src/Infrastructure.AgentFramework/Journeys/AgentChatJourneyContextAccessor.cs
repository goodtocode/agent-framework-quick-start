using Goodtocode.AgentStart.Core.Application.Chats.Journeys;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Tools;

namespace Goodtocode.AgentStart.Infrastructure.AgentFramework.Journeys;

/// <summary>
/// Projects the per-chat-session tool selection state the chat router maintains into the
/// Application-layer <see cref="ChatJourneyContext"/>, so journey resolution reads the same
/// selections that deterministic follow-up routing uses.
/// </summary>
public sealed class AgentChatJourneyContextAccessor(IAgentChatContextAccessor chatContextAccessor)
    : IChatJourneyContextAccessor
{
    private readonly IAgentChatContextAccessor _chatContextAccessor = chatContextAccessor;

    public ChatJourneyContext GetContext(Guid chatSessionId)
    {
        var toolContext = _chatContextAccessor.GetOrCreateContext(chatSessionId);

        return new ChatJourneyContext
        {
            ActorId = toolContext.ActorId,
            SelectedChatSessionId = toolContext.SelectedChatSessionId,
            MyChatSessionsListed = toolContext.MyChatSessionsListed
        };
    }
}
