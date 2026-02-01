namespace Goodtocode.AgentFramework.Core.Application.Image;

public class CreateTextToImageCommandValidator : Validator<CreateTextToImageCommand>
{
    public CreateTextToImageCommandValidator()
    {
        RuleFor(x => x.Prompt).NotEmpty();
    }
}