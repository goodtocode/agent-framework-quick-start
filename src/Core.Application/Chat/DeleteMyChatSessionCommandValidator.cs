namespace Goodtocode.AgentFramework.Core.Application.Chat;

public class DeleteMyChatSessionCommandValidator : SecuredValidator<DeleteMyChatSessionCommand>
{
    public DeleteMyChatSessionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty("Id is required");
    }
}