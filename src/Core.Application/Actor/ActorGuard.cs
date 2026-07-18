using Goodtocode.AgentFramework.Core.Domain.Actor;
using Goodtocode.AgentFramework.Core.Domain.Chat;

namespace Goodtocode.AgentFramework.Core.Application.Actor;

public static class ActorGuard
{
    public static void GuardAgainstEmptyUserContext(IUserContext? userContext)
    {
        if (userContext == null || userContext.OwnerId == Guid.Empty || userContext.TenantId == Guid.Empty)
            throw new CustomValidationException([
                new("UserContext", "A valid UserContext with OwnerId and TenantId is required to link an actor.")
            ]);
    }

    public static void GuardAgainstNotFound(ActorEntity? actor)
    {
        if (actor == null)
        {
            throw new CustomNotFoundException("Actor Not Found");
        }
    }

    public static void GuardAgainstNotFound(ChatSessionEntity? entity)
    {
        if (entity is null)
        {
            throw new CustomNotFoundException("Chat Session Not Found");
        }
    }

    public static void GuardAgainstInvalidUserContext(IUserContext? userContext)
    {
        if (userContext is null)
        {
            throw new CustomValidationException([
                new("UserContext", "UserContext is required to save an actor")
            ]);
        }

        if (userContext.OwnerId == Guid.Empty)
        {
            throw new CustomValidationException([
                new("OwnerId", "A valid OwnerId is required in UserContext to save an actor")
            ]);
        }
    }
}
