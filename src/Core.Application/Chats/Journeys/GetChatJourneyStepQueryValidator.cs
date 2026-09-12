namespace Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

public class GetChatJourneyStepQueryValidator : SecuredValidator<GetChatJourneyStepQuery>
{
    public GetChatJourneyStepQueryValidator()
    {
        RuleFor(x => x.ChatSessionId)
            .NotEmpty("ChatSessionId is required");
    }
}
