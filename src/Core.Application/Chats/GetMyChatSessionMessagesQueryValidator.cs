namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class GetMyChatSessionMessagesQueryValidator : SecuredValidator<GetMyChatSessionMessagesQuery>
{
    public GetMyChatSessionMessagesQueryValidator()
    {
        RuleFor(x => x.ChatSessionId).NotEmpty();
    }
}