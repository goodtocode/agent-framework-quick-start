namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class GetOurActorChatSessionQueryValidator : SecuredValidator<GetOurActorChatSessionQuery>
{
    public GetOurActorChatSessionQueryValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.ChatSessionId).NotEmpty();
    }
}