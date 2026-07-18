using Goodtocode.AgentFramework.Core.Application.Chat;
using Goodtocode.AgentFramework.Core.Domain.Chat;

namespace Goodtocode.AgentFramework.Tests.Integration.Chat;

[Binding]
[Scope(Tag = "getChatSessionQuery")]
public class GetMyChatSessionQueryStepDefinitions : TestBase
{
    private Guid _id;
    private bool _exists;
    private ChatSessionDto? _response;

    [Given(@"I have a definition ""([^""]*)""")]
    public void GivenIHaveADefinition(string def)
    {
        base.def = def;
    }

    [Given(@"I have a chat session id ""([^""]*)""")]
    public void GivenIHaveAChatSessionId(string chatSessionKey)
    {
        if (string.IsNullOrWhiteSpace(chatSessionKey))
        {
            return;
        }

        Guid.TryParse(chatSessionKey, out _id).ShouldBeTrue();
    }

    [Given(@"I the chat session exists ""([^""]*)""")]
    public void GivenITheChatSessionExists(string exists)
    {
        bool.TryParse(exists, out _exists).ShouldBeTrue();
    }

    [When(@"I get a chat session")]
    public async Task WhenIGetAChatSession()
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

        var request = new GetMyChatSessionQuery()
        {
            Id = _id
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
    public void ThenIfTheResponseIsSuccessfulTheResponseHasAId()
    {
        if (responseType != CommandResponseType.Successful)
        {
            return;
        }

        _response?.Id.ShouldNotBeEmpty();
    }
}
