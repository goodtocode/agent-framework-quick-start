namespace Goodtocode.AgentFramework.Core.Application.Audio;

public class CreateTextToAudioCommandValidator : Validator<CreateTextToAudioCommand>
{
    public CreateTextToAudioCommandValidator()
    {
        RuleFor(x => x.Prompt).NotEmpty();
    }
}