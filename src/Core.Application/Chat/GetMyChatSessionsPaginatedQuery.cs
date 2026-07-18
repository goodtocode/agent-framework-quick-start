namespace Goodtocode.AgentFramework.Core.Application.Chat;

public class GetMyChatSessionsPaginatedQuery : UserScopedRequest, IRequest<IPaginatedList<ChatSessionDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

}

public class GetMyChatSessionsPaginatedQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyChatSessionsPaginatedQuery, IPaginatedList<ChatSessionDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<IPaginatedList<ChatSessionDto>> Handle(GetMyChatSessionsPaginatedQuery request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyUserForQuery(request?.UserContext);
        var ownerId = request!.UserContext!.OwnerId;

        var returnData = await _context.ChatSessions
            .Include(x => x.Messages)
            .Where(x => x.OwnerId == ownerId)
            .OrderByDescending(x => x.Timestamp)
            .Where(x => (request.StartDate == null || x.Timestamp > request.StartDate)
                && (request.EndDate == null || x.Timestamp < request.EndDate))
            .Select(x => ChatSessionDto.CreateFrom(x))
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        return returnData;
    }
}