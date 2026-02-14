using Goodtocode.Domain.Entities;

namespace Goodtocode.AgentFramework.Core.Domain.ChatCompletion;

public class ChatMessageEntity : SecuredEntity<ChatMessageEntity>, IDomainEntity<ChatMessageEntity>
{
    protected ChatMessageEntity(Guid id, Guid ownerId, Guid tenantId) : base(id, ownerId, tenantId) { }
    public Guid ChatSessionId { get; private set; }
    public ChatMessageRole Role { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public virtual ChatSessionEntity? ChatSession { get; private set; }
    public static ChatMessageEntity Create(Guid id, Guid chatSessionId, ChatMessageRole role, string content, Guid ownerId, Guid tenantId)
    {
        return new ChatMessageEntity(id, ownerId, tenantId)
        {
            ChatSessionId = chatSessionId,
            Role = role,
            Content = content
        };
    }
}
