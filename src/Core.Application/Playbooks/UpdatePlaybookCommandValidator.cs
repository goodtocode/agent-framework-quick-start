namespace Goodtocode.AgentFramework.Core.Application.Playbooks;

public class UpdatePlaybookCommandValidator : Validator<UpdatePlaybookCommand>
{
    public UpdatePlaybookCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty("Id is required.");
    }
}
