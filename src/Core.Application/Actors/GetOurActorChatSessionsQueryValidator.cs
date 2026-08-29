namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class GetOurActorChatSessionsQueryValidator : SecuredValidator<GetOurActorChatSessionsQuery>
{
    public GetOurActorChatSessionsQueryValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(v => v.StartDate).NotEmpty()
            .When(v => v.EndDate != null)
            .LessThanOrEqualTo(v => v.EndDate);

        RuleFor(v => v.EndDate)
            .NotEmpty()
            .When(v => v.StartDate != null)
            .GreaterThanOrEqualTo(v => v.StartDate);
    }
}