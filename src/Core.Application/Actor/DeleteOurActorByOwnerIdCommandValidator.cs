namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class DeleteOurActorByOwnerIdCommandValidator : SecuredValidator<DeleteActorByOwnerIdCommand>
{
    public DeleteOurActorByOwnerIdCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty();
    }
}