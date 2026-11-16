namespace Goodtocode.AgentStart.Core.Application.Actors;

public class DeleteOurActorCommandValidator : SecuredValidator<DeleteOurActorCommand>
{
    public DeleteOurActorCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}