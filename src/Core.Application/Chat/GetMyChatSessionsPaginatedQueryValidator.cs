namespace Goodtocode.AgentFramework.Core.Application.Chat;

public class GetMyChatSessionsPaginatedQueryValidator : SecuredValidator<GetMyChatSessionsPaginatedQuery>
{
    public GetMyChatSessionsPaginatedQueryValidator()
    {
        RuleFor(v => v.StartDate)
            .NotEmpty("StartDate is required when EndDate is provided")
            .When(v => v.EndDate != null)
            .LessThanOrEqualTo(v => v.EndDate);

        RuleFor(v => v.EndDate)
            .NotEmpty("EndDate is required when StartDate is provided")
            .When(v => v.StartDate != null)
            .GreaterThanOrEqualTo(v => v.StartDate);

        RuleFor(x => x.PageNumber)
            .NotEqual(0, "PageNumber must be greater than 0");

        RuleFor(x => x.PageSize)
            .NotEqual(0, "PageSize must be greater than 0");
    }
}