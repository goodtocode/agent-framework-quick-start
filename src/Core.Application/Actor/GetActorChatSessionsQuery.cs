using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.ChatCompletion;

namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class GetActorChatSessionsQuery : IRequest<ICollection<ChatSessionDto>>
{
    public Guid ActorId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetActorChatSessionsQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetActorChatSessionsQuery, ICollection<ChatSessionDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ICollection<ChatSessionDto>> Handle(GetActorChatSessionsQuery request, CancellationToken cancellationToken)
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