namespace Goodtocode.AgentFramework.Core.Application.Chat;

public class GetMyChatSessionMessagesQueryValidator : SecuredValidator<GetMyChatSessionMessagesQuery>
{
    public GetMyChatSessionMessagesQueryValidator()
    {
        RuleFor(x => x.ChatSessionId).NotEmpty();
    }
}