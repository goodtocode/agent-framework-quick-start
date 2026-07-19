using Goodtocode.AgentFramework.Core.Application.Actor;
using Goodtocode.AgentFramework.Core.Domain.Actor;

namespace Goodtocode.AgentFramework.Tests.Integration.Actor
{
    [Binding]
    [Scope(Tag = "deleteActorCommand")]
    public class DeleteActorCommandStepDefinitions : TestBase
    {
        private bool _exists;
        private Guid _id;

        [Given(@"I have a def ""([^""]*)""")]
        public void GivenIHaveADef(string def)
        {
            base.def = def;
        }

        [Given(@"I have a actor id""([^""]*)""")]
        public void GivenIHaveAAuthorId(string id)
        {
            Guid.TryParse(id, out _id).ShouldBeTrue();
        }

        [Given(@"The actor exists ""([^""]*)""")]
        public void GivenTheAuthorExists(string exists)
        {
            bool.TryParse(exists, out _exists).ShouldBeTrue();
        }

        [When(@"I delete the actor")]
        public async Task WhenIDeleteTheAuthor()
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
                _id = actor.OwnerId;
            }

            var request = new DeleteActorByOwnerIdCommand()
            {
                OwnerId = _id == Guid.Empty ? rlsContext.OwnerId : _id
            };

            try
            {
                await Sender.Send(request, CancellationToken.None);
                responseType = CommandResponseType.Successful;
            }
            catch (Exception e)
            {
                HandleAssignResponseType(e);
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
    }
}
