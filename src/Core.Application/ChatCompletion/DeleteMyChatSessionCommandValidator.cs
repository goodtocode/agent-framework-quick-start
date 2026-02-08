namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class DeleteMyChatSessionCommandValidator : Validator<DeleteMyChatSessionCommand>
{
    public DeleteMyChatSessionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty("Id is required");

        RuleFor(x => x.UserContext)
            .NotEmpty("User information is required");

        RuleFor(x => x.UserContext!.OwnerId)
            .NotEmpty("OwnerId is required")
            .When(x => x.UserContext != null);

        RuleFor(x => x.UserContext!.TenantId)
            .NotEmpty("TenantId is required")
            .When(x => x.UserContext != null);
    }
}