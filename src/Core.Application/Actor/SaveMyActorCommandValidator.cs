namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class SaveMyActorCommandValidator : Validator<SaveMyActorCommand>
{
    public SaveMyActorCommandValidator()
    {
        RuleFor(x => x.UserContext).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
    }
}