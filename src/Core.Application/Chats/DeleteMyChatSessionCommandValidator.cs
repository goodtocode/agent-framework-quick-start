namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class DeleteMyChatSessionCommandValidator : SecuredValidator<DeleteMyChatSessionCommand>
{
    public DeleteMyChatSessionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty("Id is required");
    }
}