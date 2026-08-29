namespace Goodtocode.AgentFramework.Core.Application.Actors;

public class GetOurActorQueryValidator : SecuredValidator<GetOurActorQuery>
{
    public GetOurActorQueryValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
    }
}