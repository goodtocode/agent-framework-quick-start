namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class CreateChatSessionCommandValidator : Validator<CreateChatSessionCommand>
{
    public CreateChatSessionCommandValidator()
    {
        RuleFor(x => x.Message).NotEmpty();
    }
}