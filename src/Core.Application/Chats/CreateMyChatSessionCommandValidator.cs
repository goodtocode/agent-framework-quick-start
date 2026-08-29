namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class CreateMyChatSessionCommandValidator : SecuredValidator<CreateMyChatSessionCommand>
{
    public CreateMyChatSessionCommandValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty("Message is required");
    }
}