namespace Goodtocode.AgentFramework.Core.Application.Chat;

public class GetMyChatSessionsQueryValidator : SecuredValidator<GetMyChatSessionsQuery>
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
    }
}