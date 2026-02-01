namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetMyChatSessionQueryValidator : Validator<GetMyChatSessionQuery>
{
    public GetMyChatSessionQueryValidator()
    {
        RuleFor(x => x.UserInfo).NotEmpty();
        RuleFor(x => x.ChatSessionId).NotEmpty();
    }
}