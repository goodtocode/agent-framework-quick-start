using Goodtocode.AgentFramework.Core.Application.Chat;
using Goodtocode.AgentFramework.Core.Domain.Chat;

namespace Goodtocode.AgentFramework.Tests.Integration.Chat
{
    [Binding]
    [Scope(Tag = "deleteChatSessionCommand")]
    public class DeleteChatSessionCommandStepDefinitions : TestBase
    {
        private Guid _id;
        private bool _exists;

        [Given(@"I have a def ""([^""]*)""")]
        public void GivenIHaveADef(string def)
        {
            base.def = def;
        }

        [Given(@"I have a chat session id""([^""]*)""")]
        public void GivenIHaveAChatSessionKey(string id)
        {
            Guid.TryParse(id, out _id).ShouldBeTrue();
        }

        [Given(@"The chat session exists ""([^""]*)""")]
        public void GivenTheChatSessionExists(string exists)
        {
            bool.TryParse(exists, out _exists).ShouldBeTrue();
        }

        [When(@"I delete the chat session")]
        public async Task WhenIDeleteTheChatSession()
        {
            if (_exists)
            {
                var chatSession = ChatSessionEntity.Create(
                    ownerId: rlsContext.OwnerId,
                    tenantId: rlsContext.TenantId,
                    actorId: Guid.NewGuid(),
                    title: "Test Session"
                );
                context.ChatSessions.Add(chatSession);
                await context.SaveChangesAsync(CancellationToken.None);
                _id = chatSession.Id;
            }

            var request = new DeleteMyChatSessionCommand()
            {
                Id = _id
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
