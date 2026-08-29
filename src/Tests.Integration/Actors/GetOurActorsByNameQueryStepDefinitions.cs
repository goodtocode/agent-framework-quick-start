using Goodtocode.AgentFramework.Core.Application.Actors;
using Goodtocode.AgentFramework.Core.Domain.Actors;

namespace Goodtocode.AgentFramework.Tests.Integration.Actor;

[Binding]
[Scope(Tag = "getOurActorsByNameQuery")]
public class GetOurActorsByNameQueryStepDefinitions : TestBase
{
    private string _name = string.Empty;
    private bool _exists;
    private bool _otherTenantExists;
    private ICollection<ActorDto>? _response;

    [Given(@"I have a definition ""([^""]*)""")]
    public void GivenIHaveADefinition(string def)
    {
        base.def = def;
    }

    [Given(@"I search for actor name ""([^""]*)""")]
    public void GivenISearchForActorName(string name)
    {
        _name = name;
    }

    [Given(@"matching actors in my tenant exist ""([^""]*)""")]
    public void GivenMatchingActorsInMyTenantExist(string exists)
    {
        bool.TryParse(exists, out _exists).ShouldBeTrue();
    }

    [Given(@"matching actors in another tenant exist ""([^""]*)""")]
    public void GivenMatchingActorsInAnotherTenantExist(string exists)
    {
        bool.TryParse(exists, out _otherTenantExists).ShouldBeTrue();
    }

    [When(@"I get actors by name")]
    public async Task WhenIGetActorsByName()
    {
        if (_exists)
        {
            context.Actors.Add(ActorEntity.Create(
                ownerId: rlsContext.OwnerId,
                tenantId: rlsContext.TenantId,
                firstName: _name,
                lastName: "CurrentTenant",
                email: "current@example.test"));
        }

        if (_otherTenantExists)
        {
            context.Actors.Add(ActorEntity.Create(
                ownerId: Guid.NewGuid(),
                tenantId: Guid.NewGuid(),
                firstName: _name,
                lastName: "OtherTenant",
                email: "other@example.test"));
        }

        await context.SaveChangesAsync(CancellationToken.None);

        try
        {
            _response = await Sender.Send(new GetOurActorsByNameQuery
            {
                Name = _name
            }, CancellationToken.None);
            responseType = CommandResponseType.Successful;
        }
        catch (Exception exception)
        {
            responseType = HandleAssignResponseType(exception);
        }
    }

    [Then(@"The response is ""([^""]*)""")]
    public void ThenTheResponseIs(string result)
    {
        HandleHasResponseType(result);
    }

    [Then(@"If the response has validation issues I see the ""([^""]*)"" in the response")]
    public void ThenIfTheResponseHasValidationIssuesISeeTheInTheResponse(string expectedErrors)
    {
        HandleExpectedValidationErrorsAssertions(expectedErrors);
    }

    [Then(@"The actor search response count is ""([^""]*)""")]
    public void ThenTheActorSearchResponseCountIs(string count)
    {
        int.TryParse(count, out var expectedCount).ShouldBeTrue();
        _response?.Count.ShouldBe(expectedCount);
    }
}