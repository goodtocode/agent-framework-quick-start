using System.Collections.Concurrent;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

public sealed class AgentChatContextAccessor : IAgentChatContextAccessor
{
    private readonly AsyncLocal<Guid?> _currentChatSession = new();
    private readonly ConcurrentDictionary<Guid, ChatToolSessionContext> _contexts = new();

    public Guid? CurrentChatSessionId => _currentChatSession.Value;

    public void SetCurrentChatSession(Guid chatSessionId)
    {
        _currentChatSession.Value = chatSessionId;
        _contexts.TryAdd(chatSessionId, new ChatToolSessionContext());
    }

    public void ClearCurrentChatSession()
    {
        _currentChatSession.Value = null;
    }

    public ChatToolSessionContext GetOrCreateContext(Guid chatSessionId)
        => _contexts.GetOrAdd(chatSessionId, _ => new ChatToolSessionContext());

    public void UpsertContext(Guid chatSessionId, Func<ChatToolSessionContext, ChatToolSessionContext> update)
    {
        ArgumentNullException.ThrowIfNull(update);

        _contexts.AddOrUpdate(
            chatSessionId,
            _ => update(new ChatToolSessionContext()),
            (_, existing) => update(existing));
    }
}
