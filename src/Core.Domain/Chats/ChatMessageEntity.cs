namespace Goodtocode.AgentFramework.Core.Domain.Chat;

public class ChatMessageEntity : SecuredEntity<ChatMessageEntity>, IDomainEntity<ChatMessageEntity>
{
    public Guid ChatSessionId { get; private set; }
    public ChatMessageRole Role { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public virtual ChatSessionEntity? ChatSession { get; private set; }

    protected ChatMessageEntity() : base() { }

    private ChatMessageEntity(
        Guid id,
        string canonicalKey,
        Guid ownerId,
        Guid tenantId,
        Guid createdBy,
        DateTime createdOn,
        DateTimeOffset timestamp,
        Guid chatSessionId,
        ChatMessageRole role,
        string content)
        : base(id: id, partitionKey: tenantId.ToString(), rowKey: canonicalKey,
               ownerId: ownerId, tenantId: tenantId, createdBy: createdBy,
               createdOn: createdOn, timestamp: timestamp)
    {
        ChatSessionId = chatSessionId;
        Role = role;
        Content = content;
    }

    public static ChatMessageEntity Create(
        Guid ownerId,
        Guid tenantId,
        Guid chatSessionId,
        ChatMessageRole role,
        string content)
    {
        return new ChatMessageEntity(
            id: Guid.NewGuid(),
            canonicalKey: Guid.NewGuid().ToString(),
            ownerId: ownerId,
            tenantId: tenantId,
            createdBy: ownerId,
            createdOn: DateTime.UtcNow,
            timestamp: DateTimeOffset.UtcNow,
            chatSessionId: chatSessionId,
            role: role,
            content: content
        );
    }
}
