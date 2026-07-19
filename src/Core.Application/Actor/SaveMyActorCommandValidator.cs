namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class SaveMyActorCommandValidator : SecuredValidator<SaveMyActorCommand>
{
    public SaveMyActorCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty();

        RuleFor(x => x.LastName)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty();
    }
}