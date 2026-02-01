namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class PatchChatSessionCommandValidator : Validator<PatchChatSessionCommand>
{
    public PatchChatSessionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}