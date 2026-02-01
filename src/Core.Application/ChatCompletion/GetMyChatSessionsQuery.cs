using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Domain.Auth;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetMyChatSessionsQuery : IRequest<ICollection<ChatSessionDto>>, IUserInfoRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public IUserEntity? UserInfo { get; set; }
}

public class GetAuthorChatSessionsByOwnerIdQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyChatSessionsQuery, ICollection<ChatSessionDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ICollection<ChatSessionDto>> Handle(GetMyChatSessionsQuery request, CancellationToken cancellationToken)
    {
        var returnData = await _context.ChatSessions
            .OrderByDescending(x => x.Timestamp)
            .Where(x => (request.StartDate == null || x.Timestamp > request.StartDate)
                    && (request.EndDate == null || x.Timestamp < request.EndDate))
            .Join(_context.Actors,
                  cs => cs.ActorId,
                  a => a.Id,
                  (cs, a) => new { ChatSession = cs, Actor = a })
            .Where(joined => joined.Actor.OwnerId == request.UserInfo!.OwnerId)
            .Select(joined => joined.ChatSession)
            .Select(x => ChatSessionDto.CreateFrom(x))
            .ToListAsync(cancellationToken);

        return returnData;
    }
}