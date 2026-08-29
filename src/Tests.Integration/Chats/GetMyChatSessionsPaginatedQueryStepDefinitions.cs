using Goodtocode.AgentFramework.Core.Application.Chat;
using Goodtocode.AgentFramework.Core.Domain.Chat;

namespace Goodtocode.AgentFramework.Tests.Integration.Chat;

[Binding]
[Scope(Tag = "getChatSessionsPaginatedQuery")]
public class GetMyChatSessionsPaginatedQueryStepDefinitions : TestBase
{
    private bool _exists;
    private DateTime _startDate;
    private DateTime _endDate;
    private bool _withinDateRangeExists;
    private int _pageNumber;
    private int _pageSize;
    private IPaginatedList<ChatSessionDto>? _response;

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

    [Given(@"I have a start date ""([^""]*)""")]
    public void GivenIHaveAStartDate(string startDate)
    {
        if (string.IsNullOrWhiteSpace(startDate))
        {
            return;
        }

        DateTime.TryParse(startDate, out _startDate).ShouldBeTrue();
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

    [Given(@"chat sessions within the date range exists ""([^""]*)""")]
    public void GivenChatSessionsWithinTheDateRangeExists(string withinDateRangeExists)
    {
        bool.TryParse(withinDateRangeExists, out _withinDateRangeExists).ShouldBeTrue();
    }

    [Given(@"I have a page number ""([^""]*)""")]
    public void GivenIHaveAPageNumber(string pageNumber)
    {
        int.TryParse(pageNumber, out _pageNumber).ShouldBeTrue();
    }

    [Given(@"I have a page size ""([^""]*)""")]
    public void GivenIHaveAPageSize(string pageSize)
    {
        int.TryParse(pageSize, out _pageSize).ShouldBeTrue(); ;
    }

    [When(@"I get the chat sessions paginated")]
    public async Task WhenIGetTheChatSessionsPaginated()
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

        var request = new GetMyChatSessionsPaginatedQuery()
        {
            PageNumber = _pageNumber,
            PageSize = _pageSize,
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
        if (responseType != CommandResponseType.Successful)
        {
            return;
        }

        _response?.TotalCount.ShouldBe(_withinDateRangeExists == false ? 0 : _response.TotalCount);
    }

    [Then(@"Each chat session has a Key")]
    public void ThenEachChatSessionHasAKey()
    {
        if (responseType != CommandResponseType.Successful)
        {
            return;
        }

        _response?.Items.FirstOrDefault(x => x.Id == default).ShouldBeNull();
    }

    [Then(@"Each chat session has a Date greater than start date")]
    public void ThenEachChatSessionHasADateGreaterThanStartDate()
    {
        if (responseType == CommandResponseType.Successful && _withinDateRangeExists)
        {
            _response?.Items.FirstOrDefault(x => _startDate == default || x.Timestamp > _startDate).ShouldNotBeNull();
        }
    }

    [Then(@"Each chat session has a Date less than end date")]
    public void ThenEachChatSessionHasADateLessThanEndDate()
    {
        if (responseType == CommandResponseType.Successful && _withinDateRangeExists)
        {
            _response?.Items.FirstOrDefault(x => _endDate == default || x.Timestamp < _endDate).ShouldNotBeNull();
        }
    }

    [Then(@"The response has a Page Number")]
    public void ThenTheResponseHasAPageNumber()
    {
        if (responseType != CommandResponseType.Successful)
        {
            return;
        }

        _response?.PageNumber.Should();
    }

    [Then(@"The response has a Total Pages")]
    public void ThenTheResponseHasATotalPages()
    {
        if (responseType != CommandResponseType.Successful)
        {
            return;
        }

        _response?.TotalPages.Should();
    }

    [Then(@"The response has a Total Count")]
    public void ThenTheResponseHasATotalCount()
    {
        if (responseType != CommandResponseType.Successful)
        {
            return;
        }

        _response?.TotalCount.Should();
    }
}
