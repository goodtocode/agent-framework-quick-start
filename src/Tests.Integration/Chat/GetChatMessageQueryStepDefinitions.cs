using Goodtocode.AgentFramework.Core.Application.Chat;
using Goodtocode.AgentFramework.Core.Domain.Chat;

namespace Goodtocode.AgentFramework.Tests.Integration.Chat;

[Binding]
[Scope(Tag = "getChatMessageQuery")]
public class GetChatMessageQueryStepDefinitions : TestBase
{
    private Guid _id;
    private bool _exists;
    private readonly Guid _chatSessionId = Guid.NewGuid();
    private ChatMessageDto? _response;

    [Given(@"I have a definition ""([^""]*)""")]
    public void GivenIHaveADefinition(string def)
    {
        base.def = def;
    }

    [Given(@"I have a Chat Message id ""([^""]*)""")]
    public void GivenIHaveAChatMessageId(string ChatMessageKey)
    {
        if (string.IsNullOrWhiteSpace(ChatMessageKey))
        {
            return;
        }

        Guid.TryParse(ChatMessageKey, out _id).ShouldBeTrue();
    }

    [Given(@"The Chat Message exists ""([^""]*)""")]
    public void GivenITheChatMessageExists(string exists)
    {
        bool.TryParse(exists, out _exists).ShouldBeTrue();
    }

    [When(@"I get a Chat Message")]
    public async Task WhenIGetAChatMessage()
    {
        if (_exists)
        {
            var chatSession = ChatSessionEntity.Create(
                ownerId: rlsContext.OwnerId,
                tenantId: rlsContext.TenantId,
                actorId: Guid.NewGuid(),
                title: "Test Session"
            );
            var chatMessage = ChatMessageEntity.Create(
                ownerId: rlsContext.OwnerId,
                tenantId: rlsContext.TenantId,
                chatSessionId: chatSession.Id,
                role: ChatMessageRole.user,
                content: "Test Message Content"
            );
            chatSession.Messages.Add(chatMessage);
            context.ChatSessions.Add(chatSession);
            await context.SaveChangesAsync(CancellationToken.None);
            _id = chatMessage.Id;
        }

        var request = new GetMyChatMessageQuery()
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

    [Then(@"If the response is successful the response has a count matching ""([^""]*)""")]
    public void ThenIfTheResponseIsSuccessfulTheResponseHasACountMatching(string messageContent)
    {
        _response?.Content?.ShouldBe(messageContent);
    }
}
