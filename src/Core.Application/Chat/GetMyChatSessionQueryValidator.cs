namespace Goodtocode.AgentFramework.Core.Application.Chat;

public class GetMyChatSessionQueryValidator : SecuredValidator<GetMyChatSessionQuery>
{
    public GetMyChatSessionQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty("Id is required");
    }
}