namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class CreateOurActorCommandValidator : SecuredValidator<CreateOurActorCommand>
{
    public CreateOurActorCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty();

        RuleFor(x => x.LastName)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty();
    }
}