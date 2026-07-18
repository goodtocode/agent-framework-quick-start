namespace Goodtocode.AgentFramework.Core.Application.Chat;

public class GetMyChatMessagesPaginatedQuery : UserScopedRequest, IRequest<IPaginatedList<ChatMessageDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

}

public class GetMyChatMessagesPaginatedQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyChatMessagesPaginatedQuery, IPaginatedList<ChatMessageDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<IPaginatedList<ChatMessageDto>> Handle(GetMyChatMessagesPaginatedQuery request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyUserForQuery(request?.UserContext);

        var userContext = request!.UserContext!;

        var query = _context.ChatMessages
            .Where(x => x.ChatSession != null && x.ChatSession.OwnerId == userContext.OwnerId && x.ChatSession.TenantId == userContext.TenantId);

        if (request.StartDate.HasValue)
            query = query.Where(x => x.Timestamp > request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(x => x.Timestamp < request.EndDate.Value);

        var returnData = await query
            .OrderByDescending(x => x.Timestamp)
            .Select(x => ChatMessageDto.CreateFrom(x))
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        return returnData;
    }
}