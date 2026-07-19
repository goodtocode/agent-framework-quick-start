using Goodtocode.AgentFramework.Core.Application.Chat;

namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class GetOurActorChatSessionsQuery : UserScopedRequest, IRequest<ICollection<ChatSessionDto>>
{
    public Guid ActorId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

}

public class GetOurActorChatSessionsQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetOurActorChatSessionsQuery, ICollection<ChatSessionDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ICollection<ChatSessionDto>> Handle(GetOurActorChatSessionsQuery request, CancellationToken cancellationToken)
    {
        var returnData = await _context.ChatSessions
            .OrderByDescending(x => x.Timestamp)
            .Where(x => x.ActorId == request.ActorId
                    && (request.StartDate == null || x.Timestamp > request.StartDate)
                    && (request.EndDate == null || x.Timestamp < request.EndDate))
            .Select(x => ChatSessionDto.CreateFrom(x))
            .ToListAsync(cancellationToken);

        return returnData;
    }
}