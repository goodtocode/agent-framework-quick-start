namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetChatSessionQueryValidator : Validator<GetChatSessionQuery>
{
    public GetChatSessionQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}