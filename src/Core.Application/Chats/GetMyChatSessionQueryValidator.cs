namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class GetMyChatSessionQueryValidator : SecuredValidator<GetMyChatSessionQuery>
{
    public GetMyChatSessionQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty("Id is required");
    }
}