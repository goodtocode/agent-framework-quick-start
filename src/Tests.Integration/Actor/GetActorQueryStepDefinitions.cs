using Goodtocode.AgentFramework.Core.Application.Actor;
using Goodtocode.AgentFramework.Core.Domain.Actor;

namespace Goodtocode.AgentFramework.Tests.Integration.Actor;

[Binding]
[Scope(Tag = "getAuthorQuery")]
public class GetActorQueryStepDefinitions : TestBase
{
    private Guid _id;
    private bool _exists;
    private ActorDto? _response;

    [Given(@"I have a definition ""([^""]*)""")]
    public void GivenIHaveADefinition(string def)
    {
        base.def = def;
    }

    [Given(@"I have a Actor id ""([^""]*)""")]
    public void GivenIHaveAAuthorId(string actorId)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            return;
        }

        Guid.TryParse(actorId, out _id).ShouldBeTrue();
    }

    [Given(@"the Actor exists ""([^""]*)""")]
    public void GivenITheAuthorExists(string exists)
    {
        bool.TryParse(exists, out _exists).ShouldBeTrue();
    }

    [When(@"I get the Actor")]
    public async Task WhenIGetAAuthor()
    {
        if (_exists)
        {
            var actor = ActorEntity.Create(
                firstName: "John",
                lastName: "Doe",
                email: "jdoe@goodtocode.com",
                ownerId: rlsContext.OwnerId,
                tenantId: rlsContext.TenantId
            );
            context.Actors.Add(actor);
            await context.SaveChangesAsync(CancellationToken.None);
            _id = actor.Id;
        }

        var request = new GetOurActorQuery()
        {
            ActorId = _id
        };

        try
        {
            _response = await Sender.Send(request, CancellationToken.None);
            responseType = CommandResponseType.Successful;
        }
        catch (Exception e)
        {
            responseType = HandleAssignResponseType(e);
        }
    }

    [Then(@"The response is ""([^""]*)""")]
    public void ThenTheResponseIs(string response)
    {
        HandleHasResponseType(response);
    }

    [Then(@"If the response has validation issues I see the ""([^""]*)"" in the response")]
    public void ThenIfTheResponseHasValidationIssuesISeeTheInTheResponse(string expectedErrors)
    {
        HandleExpectedValidationErrorsAssertions(expectedErrors);
    }

    [Then(@"If the response is successful the response has a Id")]
    public void ThenIfTheResponseIsSuccessfulTheResponseHasAKey()
    {
        if (responseType != CommandResponseType.Successful)
        {
            return;
        }

        _response?.Id.ShouldNotBeEmpty();
    }
}
