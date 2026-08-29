namespace Goodtocode.AgentFramework.Core.Application.Chat;

public class GetMyChatSessionsQuery : UserScopedRequest, IRequest<ICollection<ChatSessionDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

}

public class GetMyChatSessionsQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyChatSessionsQuery, ICollection<ChatSessionDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ICollection<ChatSessionDto>> Handle(GetMyChatSessionsQuery request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyUserForQuery(request?.UserContext);

        var startDate = request?.StartDate;
        var endDate = request?.EndDate;
        var userContext = request?.UserContext;

        var returnData = await _context.ChatSessions
            .Where(x => userContext != null && x.OwnerId == userContext.OwnerId && x.TenantId == userContext.TenantId)
            .OrderByDescending(x => x.Timestamp)
            .Where(x => (startDate == null || x.Timestamp > startDate)
                    && (endDate == null || x.Timestamp < endDate))
            .Include(x => x.Messages)
            .Select(x => ChatSessionDto.CreateFrom(x))
            .ToListAsync(cancellationToken);

        return returnData;
    }
}