namespace Goodtocode.AgentFramework.Core.Application.Chat;

public class CreateMyChatMessageCommandValidator : SecuredValidator<CreateMyChatMessageCommand>
{
    public CreateMyChatMessageCommandValidator()
    {
        RuleFor(x => x.ChatSessionId)
            .NotEmpty("ChatSessionId is required");

        RuleFor(x => x.Message)
            .NotEmpty("Message is required");
    }
}