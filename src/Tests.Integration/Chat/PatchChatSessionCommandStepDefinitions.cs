using Goodtocode.AgentFramework.Core.Application.Chat;
using Goodtocode.AgentFramework.Core.Domain.Chat;

namespace Goodtocode.AgentFramework.Tests.Integration.Chat
{
    [Binding]
    [Scope(Tag = "patchChatSessionCommand")]
    public class PatchChatSessionCommandStepDefinitions : TestBase
    {
        private Guid _id;
        private bool _exists;
        private string _title = string.Empty;

        [Given(@"I have a def ""([^""]*)""")]
        public void GivenIHaveADef(string def)
        {
            base.def = def;
        }

        [Given(@"I have a chat session id ""([^""]*)""")]
        public void GivenIHaveAChatSessionId(string id)
        {
            Guid.TryParse(id, out _id).ShouldBeTrue();
        }

        [Given(@"the chat session exists ""([^""]*)""")]
        public void GivenTheChatSessionExists(string exists)
        {
            _exists = bool.Parse(exists);
        }

        [Given(@"I have a new chat session title ""([^""]*)""")]
        public void GivenIHaveANewChatSessionTitle(string title)
        {
            _title = title;
        }

        [When(@"I patch the chatSession")]
        public async Task WhenIPatchTheChatSession()
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

            var request = new PatchMyChatSessionCommand()
            {
                Id = _id,
                Title = _title
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
