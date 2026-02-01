namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class DeleteChatSessionCommandValidator : Validator<DeleteChatSessionCommand>
{
    public DeleteChatSessionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}