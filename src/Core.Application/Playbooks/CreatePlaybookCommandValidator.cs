namespace Goodtocode.AgentFramework.Core.Application.Playbooks;

public class CreatePlaybookCommandValidator : Validator<CreatePlaybookCommand>
{
    public CreatePlaybookCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty("Key is required.");
        RuleFor(x => x.Name).NotEmpty("Name is required.");
        RuleFor(x => x.Description).NotEmpty("Description is required.");
        RuleFor(x => x.WorkflowType).NotEmpty("WorkflowType is required.");
        RuleFor(x => x.Version).NotEmpty("Version is required.");
        RuleFor(x => x.Collect).NotEmpty("Collect step is required.");
        RuleFor(x => x.Evaluate).NotEmpty("Evaluate step is required.");
        RuleFor(x => x.Record).NotEmpty("Record step is required.");
    }
}
