namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetMyChatSessionQueryValidator : Validator<GetMyChatSessionQuery>
{
    public GetMyChatSessionQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty("ChatSessionId is required");

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