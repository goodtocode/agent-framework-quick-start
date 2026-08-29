using Goodtocode.AgentFramework.Core.Application.Chats;
using Goodtocode.AgentFramework.Core.Domain.Chats;

namespace Goodtocode.AgentFramework.Tests.Integration.Chat;

[Binding]
[Scope(Tag = "getMyChatSessionMessagesQuery")]
public class GetMyChatSessionMessagesQueryStepDefinitions : TestBase
{
    private string _requestedSession = string.Empty;
    private bool _exists;
    private bool _otherOwnerExists;
    private Guid _currentSessionId;
    private Guid _otherSessionId;
    private ICollection<ChatMessageDto>? _response;

    [Given(@"I have a definition ""([^""]*)""")]
    public void GivenIHaveADefinition(string def)
    {
        base.def = def;
    }

    [Given(@"the requested session is ""([^""]*)""")]
    public void GivenTheRequestedSessionIs(string requestedSession)
    {
        _requestedSession = requestedSession;
    }

    [Given(@"messages in my chat session exist ""([^""]*)""")]
    public void GivenMessagesInMyChatSessionExist(string exists)
    {
        bool.TryParse(exists, out _exists).ShouldBeTrue();
    }

    [Given(@"messages in another owner's chat session exist ""([^""]*)""")]
    public void GivenMessagesInAnotherOwnersChatSessionExist(string exists)
    {
        bool.TryParse(exists, out _otherOwnerExists).ShouldBeTrue();
    }

    [When(@"I get my chat session messages")]
    public async Task WhenIGetMyChatSessionMessages()
    {
        if (_exists)
        {
            var session = ChatSessionEntity.Create(
                ownerId: rlsContext.OwnerId,
                tenantId: rlsContext.TenantId,
                actorId: Guid.NewGuid(),
                title: "Current session");
            var message = ChatMessageEntity.Create(
                ownerId: rlsContext.OwnerId,
                tenantId: rlsContext.TenantId,
                chatSessionId: session.Id,
                role: ChatMessageRole.user,
                content: "Current owner message");
            session.Messages.Add(message);
            context.ChatSessions.Add(session);
            _currentSessionId = session.Id;
        }

        if (_otherOwnerExists)
        {
            var otherOwnerId = Guid.NewGuid();
            var session = ChatSessionEntity.Create(
                ownerId: otherOwnerId,
                tenantId: rlsContext.TenantId,
                actorId: Guid.NewGuid(),
                title: "Other owner session");
            var message = ChatMessageEntity.Create(
                ownerId: otherOwnerId,
                tenantId: rlsContext.TenantId,
                chatSessionId: session.Id,
                role: ChatMessageRole.user,
                content: "Other owner message");
            session.Messages.Add(message);
            context.ChatSessions.Add(session);
            _otherSessionId = session.Id;
        }

        await context.SaveChangesAsync(CancellationToken.None);

        var chatSessionId = _requestedSession switch
        {
            "current" => _currentSessionId,
            "other" => _otherSessionId,
            _ => Guid.Empty
        };

        try
        {
            _response = await Sender.Send(new GetMyChatSessionMessagesQuery
            {
                ChatSessionId = chatSessionId
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

    [Then(@"The chat session message response count is ""([^""]*)""")]
    public void ThenTheChatSessionMessageResponseCountIs(string count)
    {
        int.TryParse(count, out var expectedCount).ShouldBeTrue();
        _response?.Count.ShouldBe(expectedCount);
    }
}