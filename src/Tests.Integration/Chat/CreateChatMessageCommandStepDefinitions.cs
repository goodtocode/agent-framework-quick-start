using Goodtocode.AgentFramework.Core.Application.Chat;
using Goodtocode.AgentFramework.Core.Domain.Actor;
using Goodtocode.AgentFramework.Core.Domain.Chat;

namespace Goodtocode.AgentFramework.Tests.Integration.Chat;

[Binding]
[Scope(Tag = "createChatMessageCommand")]
public class CreateChatMessageCommandStepDefinitions : TestBase
{
    private string _message = string.Empty;
    private Guid _id;
    private Guid _chatSessionId;
    private bool _exists;

    [Given(@"I have a def ""([^""]*)""")]
    public void GivenIHaveADef(string def)
    {
        base.def = def;
    }

    [Given(@"I have a initial message ""([^""]*)""")]
    public void GivenIHaveAInitialMessage(string message)
    {
        _message = message;
    }

    [Given(@"I have a Chat Message id ""([^""]*)""")]
    public void GivenIHaveAChatMessageKey(string id)
    {
        _id = Guid.Parse(id);
    }

    [Given(@"The Chat Message exists ""([^""]*)""")]
    public void GivenTheChatMessageExists(string exists)
    {
        _exists = bool.Parse(exists);
    }

    [When(@"I create a Chat Message with the message")]
    public async Task WhenICreateAChatMessageWithTheMessage()
    {
        if (_exists)
        {
            var actor = ActorEntity.Create(
                firstName: "John",
                lastName: "Doe",
                email: "John.Doe@goodtocode.com",
                ownerId: rlsContext.OwnerId,
                tenantId: rlsContext.TenantId
            );
            context.Actors.Add(actor);
            await context.SaveChangesAsync(CancellationToken.None);
            var chatSession = ChatSessionEntity.Create(
                ownerId: rlsContext.OwnerId,
                tenantId: rlsContext.TenantId,
                actorId: actor.Id,
                title: "Test Session"
            );
            context.ChatSessions.Add(chatSession);
            await context.SaveChangesAsync(CancellationToken.None);
            _chatSessionId = chatSession.Id;
        }

        var request = new CreateMyChatMessageCommand()
        {
            ChatSessionId = _chatSessionId,
            Message = _message
        };

        try
        {
            var created = await Sender.Send(request, CancellationToken.None);
            _id = created.Id;
            responseType = CommandResponseType.Successful;
        }
        catch (Exception e)
        {
            HandleAssignResponseType(e);
        }
    }

    [Then(@"I see the Chat Message created with the initial response ""([^""]*)""")]
    public void ThenISeeTheChatMessageCreatedWithTheInitialResponse(string response)
    {
        HandleHasResponseType(response);
    }

    [Then(@"if the response has validation issues I see the ""([^""]*)"" in the response")]
    public void ThenIfTheResponseHasValidationIssuesISeeTheInTheResponse(string expectedErrors)
    {
        HandleExpectedValidationErrorsAssertions(expectedErrors);
    }
}
