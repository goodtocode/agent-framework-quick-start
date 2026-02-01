using Goodtocode.AgentFramework.Core.Application.Abstractions;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetChatSessionsQuery : IRequest<ICollection<ChatSessionDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetChatSessionsQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetChatSessionsQuery, ICollection<ChatSessionDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ICollection<ChatSessionDto>> Handle(GetChatSessionsQuery request, CancellationToken cancellationToken)
    {
        var returnData = await _context.ChatSessions
            .OrderByDescending(x => x.Timestamp)
            .Where(x => (request.StartDate == null || x.Timestamp > request.StartDate)
                    && (request.EndDate == null || x.Timestamp < request.EndDate))
            .Select(x => ChatSessionDto.CreateFrom(x))
            .ToListAsync(cancellationToken);

        return returnData;
    }
}