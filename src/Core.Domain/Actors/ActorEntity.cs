namespace Goodtocode.AgentFramework.Core.Domain.Actor;

public class ActorEntity : SecuredEntity<ActorEntity>
{
    public string? FirstName { get; private set; } = string.Empty;
    public string? LastName { get; private set; } = string.Empty;
    public string? Email { get; private set; } = string.Empty;

    protected ActorEntity() : base() { }

    private ActorEntity(
        Guid id,
        string canonicalKey,
        Guid ownerId,
        Guid tenantId,
        Guid createdBy,
        DateTime createdOn,
        DateTimeOffset timestamp,
        string? firstName,
        string? lastName,
        string? email)
        : base(id: id, partitionKey: tenantId.ToString(), rowKey: canonicalKey,
               ownerId: ownerId, tenantId: tenantId, createdBy: createdBy,
               createdOn: createdOn, timestamp: timestamp)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public static ActorEntity Create(
        Guid ownerId,
        Guid tenantId,
        string? firstName,
        string? lastName,
        string? email)
    {
        return new ActorEntity(
            id: Guid.NewGuid(),
            canonicalKey: Guid.NewGuid().ToString(),
            ownerId: ownerId,
            tenantId: tenantId,
            createdBy: ownerId,
            createdOn: DateTime.UtcNow,
            timestamp: DateTimeOffset.UtcNow,
            firstName: firstName,
            lastName: lastName,
            email: email
        );
    }

    public void Update(string? firstName, string? lastName, string? email)
    {
        FirstName = firstName ?? FirstName;
        LastName = lastName ?? LastName;
        Email = email ?? Email;
    }
}
