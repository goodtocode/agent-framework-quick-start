namespace Goodtocode.AgentFramework.Core.Application.TextGeneration;

public class DeleteTextPromptCommandValidator : Validator<DeleteTextPromptCommand>
{
    public DeleteTextPromptCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}