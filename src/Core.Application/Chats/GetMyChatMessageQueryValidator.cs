namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class GetMyChatMessageQueryValidator : SecuredValidator<GetMyChatMessageQuery>
{
    public GetMyChatMessageQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty("Id is required");
    }
}