namespace Goodtocode.AgentFramework.Core.Application.Image;

public class GetTextImageQueryValidator : Validator<GetTextImageQuery>
{
    public GetTextImageQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}