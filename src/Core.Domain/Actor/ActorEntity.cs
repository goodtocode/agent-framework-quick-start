using Goodtocode.AgentFramework.Core.Domain.Auth;
using Goodtocode.Domain.Entities;

namespace Goodtocode.AgentFramework.Core.Domain.Actor;

public class ActorEntity : SecuredEntity<ActorEntity>
{
    protected ActorEntity() : base(Guid.Empty, Guid.Empty, Guid.Empty) {    }
    public string? FirstName { get; private set; } = string.Empty;
    public string? LastName { get; private set; } = string.Empty;
    public string? Email { get; private set; } = string.Empty;

    public static ActorEntity Create(Guid id, string? firstName, string? lastName, string? email, Guid ownerId, Guid tenantId)
    {
        return new ActorEntity(id, firstName, lastName, email, ownerId, tenantId);
    }

    private ActorEntity(Guid id, string? firstName, string? lastName, string? email, Guid ownerId, Guid tenantId) : base(id, ownerId, tenantId)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public static ActorEntity Create(IUserContext userInfo)
    {
        return new ActorEntity(
            Guid.NewGuid(),
            userInfo.FirstName,
            userInfo.LastName,
            userInfo.Email,
            userInfo.OwnerId,
            userInfo.TenantId
        );
    }

    public void Update(string? firstName, string? lastName, string? email)
    {
        FirstName = firstName ?? FirstName;
        LastName = lastName ?? LastName;
        Email = email ?? Email;
    }
}
