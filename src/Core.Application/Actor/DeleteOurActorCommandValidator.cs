namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class DeleteOurActorCommandValidator : SecuredValidator<DeleteOurActorCommand>
{
    public DeleteOurActorCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}