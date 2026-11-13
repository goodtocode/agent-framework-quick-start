using Goodtocode.AgentStart.Core.Application.Chats;

namespace Goodtocode.AgentStart.Core.Application.Actors;

public class GetOurActorChatSessionsPaginatedQuery : UserScopedRequest, IRequest<IPaginatedList<ChatSessionDto>>
{
    public Guid ActorId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

}

public class GetOurActorChatSessionsPaginatedQueryHandler(IApplicationDbContext context) : IRequestHandler<GetOurActorChatSessionsPaginatedQuery, IPaginatedList<ChatSessionDto>>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<IPaginatedList<ChatSessionDto>> Handle(GetOurActorChatSessionsPaginatedQuery request, CancellationToken cancellationToken)
    {
        var returnData = await _context.ChatSessions
            .Include(x => x.Messages)
            .OrderByDescending(x => x.Timestamp)
            .Where(x => (request.StartDate == null || x.Timestamp > request.StartDate)
                && (request.EndDate == null || x.Timestamp < request.EndDate))
            .Select(x => ChatSessionDto.CreateFrom(x))
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        return returnData;

    }
}