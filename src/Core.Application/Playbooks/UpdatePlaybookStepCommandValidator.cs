namespace Goodtocode.AgentStart.Core.Application.Playbooks;

public class UpdatePlaybookStepCommandValidator : Validator<UpdatePlaybookStepCommand>
{
    public UpdatePlaybookStepCommandValidator()
    {
        RuleFor(x => x.PlaybookId).NotEmpty("PlaybookId is required.");
        RuleFor(x => x.StepType).IsInEnum();
    }
}
