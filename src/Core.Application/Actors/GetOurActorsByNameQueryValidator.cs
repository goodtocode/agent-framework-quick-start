namespace Goodtocode.AgentFramework.Core.Application.Actors;

public class GetOurActorsByNameQueryValidator : SecuredValidator<GetOurActorsByNameQuery>
{
    public GetOurActorsByNameQueryValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}