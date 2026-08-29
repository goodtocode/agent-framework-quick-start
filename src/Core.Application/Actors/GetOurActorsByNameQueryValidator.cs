namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class GetOurActorsByNameQueryValidator : SecuredValidator<GetOurActorsByNameQuery>
{
    public GetOurActorsByNameQueryValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}