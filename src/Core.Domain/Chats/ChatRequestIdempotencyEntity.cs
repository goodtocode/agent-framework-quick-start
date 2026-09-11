namespace Goodtocode.AgentFramework.Core.Domain.Chats;

public class ChatRequestIdempotencyEntity : SecuredEntity<ChatRequestIdempotencyEntity>
{
    public string Operation { get; private set; } = string.Empty;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string RequestHash { get; private set; } = string.Empty;
    public Guid? ResourceId { get; private set; }
    public Guid? ChatSessionId { get; private set; }

    protected ChatRequestIdempotencyEntity() : base() { }

    private ChatRequestIdempotencyEntity(
        Guid id,
        string canonicalKey,
        Guid ownerId,
        Guid tenantId,
        Guid createdBy,
        DateTime createdOn,
        DateTimeOffset timestamp,
        string operation,
        string idempotencyKey,
        string requestHash,
        Guid? resourceId,
        Guid? chatSessionId)
        : base(id: id, partitionKey: tenantId.ToString(), rowKey: canonicalKey,
               ownerId: ownerId, tenantId: tenantId, createdBy: createdBy,
               createdOn: createdOn, timestamp: timestamp)
    {
        Operation = operation;
        IdempotencyKey = idempotencyKey;
        RequestHash = requestHash;
        ResourceId = resourceId;
        ChatSessionId = chatSessionId;
    }

    public static ChatRequestIdempotencyEntity Create(
        Guid ownerId,
        Guid tenantId,
        string operation,
        string idempotencyKey,
        string requestHash,
        Guid? resourceId,
        Guid? chatSessionId)
    {
        return new ChatRequestIdempotencyEntity(
            id: Guid.NewGuid(),
            canonicalKey: Guid.NewGuid().ToString(),
            ownerId: ownerId,
            tenantId: tenantId,
            createdBy: ownerId,
            createdOn: DateTime.UtcNow,
            timestamp: DateTimeOffset.UtcNow,
            operation: operation,
            idempotencyKey: idempotencyKey,
            requestHash: requestHash,
            resourceId: resourceId,
            chatSessionId: chatSessionId);
    }
}
