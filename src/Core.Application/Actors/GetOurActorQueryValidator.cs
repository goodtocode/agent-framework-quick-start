namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class GetOurActorQueryValidator : SecuredValidator<GetOurActorQuery>
{
    public GetOurActorQueryValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
    }
}