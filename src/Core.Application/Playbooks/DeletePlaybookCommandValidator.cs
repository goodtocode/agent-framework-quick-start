namespace Goodtocode.AgentStart.Core.Application.Playbooks;

public class DeletePlaybookCommandValidator : Validator<DeletePlaybookCommand>
{
    public DeletePlaybookCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty("Id is required.");
    }
}
