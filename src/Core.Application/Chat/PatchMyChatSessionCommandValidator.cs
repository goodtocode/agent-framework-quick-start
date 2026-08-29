namespace Goodtocode.AgentFramework.Core.Application.Chats;

public class PatchMyChatSessionCommandValidator : SecuredValidator<PatchMyChatSessionCommand>
{
    public PatchMyChatSessionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty("Id is required");

        RuleFor(x => x.Title)
            .NotEmpty("Title is required");
    }
}