using Goodtocode.AgentFramework.Core.Application.ChatCompletion;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;

namespace Goodtocode.AgentFramework.Tests.Integration.ChatCompletion
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
            var request = new DeleteMyChatSessionCommand()
            {
                Id = _id,
                UserContext = userContext
            };

            if (_exists)
            {
                var chatSession = ChatSessionEntity.Create(_id, Guid.NewGuid(), "Test Session", ChatMessageRole.assistant, "First Message", "First Response", userContext.OwnerId, userContext.TenantId);
                context.ChatSessions.Add(chatSession);
                await context.SaveChangesAsync(CancellationToken.None);
            }

            var validator = new DeleteMyChatSessionCommandValidator();
            validationResponse = await validator.ValidateAsync(request);

            if (validationResponse.IsValid)
                try
                {
                    var handler = new DeleteChatSessionCommandHandler(context);
                    await handler.Handle(request, CancellationToken.None);
                    responseType = CommandResponseType.Successful;
                }
                catch (Exception e)
                {
                    HandleAssignResponseType(e);
                }
            else
                responseType = CommandResponseType.BadRequest;
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
