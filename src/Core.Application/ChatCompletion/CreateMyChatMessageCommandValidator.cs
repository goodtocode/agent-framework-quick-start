namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class CreateMyChatMessageCommandValidator : Validator<CreateMyChatMessageCommand>
{
    public CreateMyChatMessageCommandValidator()
    {
        RuleFor(x => x.ChatSessionId)
            .NotEmpty("ChatSessionId is required");

        RuleFor(x => x.Message)
            .NotEmpty("Message is required");

        RuleFor(x => x.UserContext)
            .NotEmpty("User information is required");

        RuleFor(x => x.UserContext!.OwnerId)
            .NotEmpty("OwnerId is required")
            .When(x => x.UserContext != null);

        RuleFor(x => x.UserContext!.TenantId)
            .NotEmpty("TenantId is required")
            .When(x => x.UserContext != null);
    }
}