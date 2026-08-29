namespace Goodtocode.AgentFramework.Core.Application.Actors;

public class DeleteOurActorByOwnerIdCommandValidator : SecuredValidator<DeleteActorByOwnerIdCommand>
{
    public DeleteOurActorByOwnerIdCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty();
    }
}