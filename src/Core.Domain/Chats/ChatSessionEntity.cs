namespace Goodtocode.AgentFramework.Core.Domain.Chat;

public class ChatSessionEntity : SecuredEntity<ChatSessionEntity>
{
    public Guid ActorId { get; private set; }
    public Guid PersonaId { get; private set; }
    public int PersonaVersion { get; private set; }
    public string? Title { get; private set; } = string.Empty;
    public virtual ICollection<ChatMessageEntity> Messages { get; private set; } = [];

    protected ChatSessionEntity() : base() { }

    private ChatSessionEntity(
        Guid id,
        string canonicalKey,
        Guid ownerId,
        Guid tenantId,
        Guid createdBy,
        DateTime createdOn,
        DateTimeOffset timestamp,
        Guid actorId,
        Guid personaId,
        int personaVersion,
        string? title,
        ICollection<ChatMessageEntity> messages)
        : base(id: id, partitionKey: tenantId.ToString(), rowKey: canonicalKey,
               ownerId: ownerId, tenantId: tenantId, createdBy: createdBy,
               createdOn: createdOn, timestamp: timestamp)
    {
        ActorId = actorId;
        PersonaId = personaId;
        PersonaVersion = personaVersion;
        Title = title;
        Messages = messages;
    }

    public static ChatSessionEntity Create(
        Guid ownerId,
        Guid tenantId,
        Guid actorId,
        string? title,
        ICollection<ChatMessageEntity>? messages = null,
        Guid? personaId = null,
        int? personaVersion = null)
    {
        return new ChatSessionEntity(
            id: Guid.NewGuid(),
            canonicalKey: Guid.NewGuid().ToString(),
            ownerId: ownerId,
            tenantId: tenantId,
            createdBy: ownerId,
            createdOn: DateTime.UtcNow,
            timestamp: DateTimeOffset.UtcNow,
            actorId: actorId,
            personaId: personaId ?? Guid.Empty,
            personaVersion: personaVersion ?? 0,
            title: title,
            messages: messages ?? []
        );
    }

    public void Update(string? title)
    {
        Title = title ?? Title;
    }
}
