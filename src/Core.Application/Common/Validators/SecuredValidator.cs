namespace Goodtocode.AgentFramework.Core.Application.Common.Validators;

public abstract class SecuredValidator<TCommand> : Validator<TCommand>
    where TCommand : IRequiresUserContext
{
    protected SecuredValidator()
    {
        RuleFor(x => x.UserContext)
            .NotEmpty("User context is required");

        RuleFor(x => x.UserContext!.OwnerId)
            .NotEmpty("OwnerId is required")
            .When(x => x.UserContext != null);

        RuleFor(x => x.UserContext!.TenantId)
            .NotEmpty("TenantId is required")
            .When(x => x.UserContext != null);
    }
}
