namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetMyChatSessionsQueryValidator : Validator<GetMyChatSessionsQuery>
{
    public GetMyChatSessionsQueryValidator()
    {
        RuleFor(v => v.StartDate)
            .NotEmpty("StartDate is required when EndDate is provided")
            .When(v => v.EndDate != null)
            .LessThanOrEqualTo(v => v.EndDate);

        RuleFor(v => v.EndDate)
            .NotEmpty("EndDate is required when StartDate is provided")
            .When(v => v.StartDate != null)
            .GreaterThanOrEqualTo(v => v.StartDate);

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