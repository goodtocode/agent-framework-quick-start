using Goodtocode.AgentFramework.Core.Application.Chat;
using Goodtocode.AgentFramework.Core.Domain.Chat;

namespace Goodtocode.AgentFramework.Tests.Integration.Chat;

[Binding]
[Scope(Tag = "getChatSessionsQuery")]
public class GetMyChatSessionsQueryStepDefinitions : TestBase
{
    private bool _exists;
    private bool _withinDateRangeExists;
    private DateTime _endDate;
    private DateTime _startDate;
    private ICollection<ChatSessionDto>? _response;

    [Given(@"I have a definition ""([^""]*)""")]
    public void GivenIHaveADefinition(string def)
    {
        base.def = def;
    }

    [Given(@"Chat Sessions exist ""([^""]*)""")]
    public void GivenChatSessionsExist(string exists)
    {
        bool.TryParse(exists, out _exists).ShouldBeTrue();
    }

    [Given(@"chat sessions within the date range exists ""([^""]*)""")]
    public void GivenChatSessionsWithinTheDateRangeExists(string withinDateRangeExists)
    {
        bool.TryParse(withinDateRangeExists, out _withinDateRangeExists).ShouldBeTrue();
    }

    [Given(@"I have a start date ""([^""]*)""")]
    public void GivenIHaveAStartDate(string startDate)
    {
        if (string.IsNullOrWhiteSpace(startDate))
        {
            return;
        }

        DateTime.TryParse(startDate, out _startDate).ShouldBeTrue();
        _startDate = DateTime.UtcNow.AddMinutes(_withinDateRangeExists ? -1 : 1);
    }

    [Given(@"I have a end date ""([^""]*)""")]
    public void GivenIHaveAEndDate(string endDate)
    {
        if (string.IsNullOrWhiteSpace(endDate))
        {
            return;
        }

        DateTime.TryParse(endDate, out _endDate).ShouldBeTrue();
    }

    [When(@"I get the chat sessions")]
    public async Task WhenIGetTheChatSessions()
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
        }

        var request = new GetMyChatSessionsQuery()
        {
            StartDate = _startDate == default ? null : _startDate,
            EndDate = _endDate == default ? null : _endDate
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

    [Then(@"The response has a collection of chat sessions")]
    public void ThenTheResponseHasACollectionOfChatSessions()
    {
        _response?.Count.ShouldBe(_withinDateRangeExists == false ? 0 : _response.Count);
    }

    [Then(@"Each chat session has a Key")]
    public void ThenEachChatSessionHasAKey()
    {
        _response?.FirstOrDefault(x => x.Id == default).ShouldBeNull();
    }

    [Then(@"Each chat session has a Date greater than start date")]
    public void ThenEachChatSessionHasADateGreaterThanStartDate()
    {
        if (_withinDateRangeExists)
        {
            _response?.FirstOrDefault(x => _startDate == default || x.Timestamp > _startDate).ShouldNotBeNull();
        }
    }

    [Then(@"Each chat session has a Date less than end date")]
    public void ThenEachChatSessionHasADateLessThanEndDate()
    {
        if (_withinDateRangeExists)
        {
            _response?.FirstOrDefault(x => _endDate == default || x.Timestamp < _endDate).ShouldNotBeNull();
        }
    }
}
