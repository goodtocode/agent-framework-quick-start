namespace Goodtocode.AgentStart.Core.Application.Actors;

public class GetOurActorsByNameQueryValidator : SecuredValidator<GetOurActorsByNameQuery>
{
    public GetOurActorsByNameQueryValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}