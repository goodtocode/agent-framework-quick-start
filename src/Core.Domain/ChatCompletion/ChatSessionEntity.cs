using Goodtocode.AgentFramework.Core.Domain.Actor;
using Goodtocode.Domain.Entities;

namespace Goodtocode.AgentFramework.Core.Domain.ChatCompletion;

public class ChatSessionEntity : SecuredEntity<ChatSessionEntity>
{
    protected ChatSessionEntity() : base(Guid.Empty, Guid.Empty, Guid.Empty) { }
    private ChatSessionEntity(Guid id, Guid ownerId, Guid tenantId) : base(id, ownerId, tenantId) { }

    public Guid ActorId { get; private set; }
    public string? Title { get; private set; } = string.Empty;
    public virtual ICollection<ChatMessageEntity> Messages { get; private set; } = [];

    public static ChatSessionEntity Create(Guid id, Guid actorId, string? title, ChatMessageRole responseRole, string initialMessage, string responseMessage, Guid ownerId, Guid tenantId)
    {
        var session = new ChatSessionEntity(id, ownerId, tenantId)
        {
            ActorId = actorId,
            Title = title,
        };
        session.Messages.Add(ChatMessageEntity.Create(Guid.NewGuid(), session.Id, ChatMessageRole.user, initialMessage, ownerId, tenantId));
        session.Messages.Add(ChatMessageEntity.Create(Guid.NewGuid(), session.Id, responseRole, responseMessage, ownerId, tenantId));
        return session;
    }

    public void Update(string? title)
    {
        Title = title ?? Title;
    }
}
