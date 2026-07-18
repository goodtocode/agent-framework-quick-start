using Goodtocode.AgentFramework.Core.Application.Chat;
using Goodtocode.AgentFramework.Core.Domain.Actor;
using Goodtocode.AgentFramework.Core.Domain.Chat;

namespace Goodtocode.AgentFramework.Tests.Integration.Chat;

[Binding]
[Scope(Tag = "createChatSessionCommand")]
public class CreateChatSessionCommandStepDefinitions : TestBase
{
    private string _message = string.Empty;
    private Guid _id;
    private readonly Guid _actorId = Guid.NewGuid();
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

    [Given(@"I have a chat session id ""([^""]*)""")]
    public void GivenIHaveAChatSessionKey(string id)
    {
        _id = Guid.Parse(id);
    }

    [Given(@"The chat session exists ""([^""]*)""")]
    public void GivenTheChatSessionExists(string exists)
    {
        _exists = bool.Parse(exists);
    }

    [When(@"I create a chat session with the message")]
    public async Task WhenICreateAChatSessionWithTheMessage()
    {
        // Setup the database if want to test existing records
        var actor = ActorEntity.Create(
            firstName: "Test",
            lastName: "Actor",
            email: "actor@goodtocode.com",
            ownerId: rlsContext.OwnerId,
            tenantId: rlsContext.TenantId
        );
        context.Actors.Add(actor);
        if (_exists)
        {
            var chatSession = ChatSessionEntity.Create(
                ownerId: rlsContext.OwnerId,
                tenantId: rlsContext.TenantId,
                actorId: _actorId,
                title: "Test Session"
            );
            context.ChatSessions.Add(chatSession);
        }
        await context.SaveChangesAsync(CancellationToken.None);

        var request = new CreateMyChatSessionCommand()
        {
            Title = def,
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

    [Then(@"I see the chat session created with the initial response ""([^""]*)""")]
    public void ThenISeeTheChatSessionCreatedWithTheInitialResponse(string response)
    {
        HandleHasResponseType(response);
    }

    [Then(@"if the response has validation issues I see the ""([^""]*)"" in the response")]
    public void ThenIfTheResponseHasValidationIssuesISeeTheInTheResponse(string expectedErrors)
    {
        HandleExpectedValidationErrorsAssertions(expectedErrors);
    }
}
