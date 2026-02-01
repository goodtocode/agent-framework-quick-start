using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.ChatCompletion;
using Goodtocode.AgentFramework.Core.Application.Common.Mappings;
using Goodtocode.AgentFramework.Core.Application.Common.Models;

namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class GetActorChatSessionsPaginatedQuery : IRequest<PaginatedList<ChatSessionDto>>
{
    public Guid ActorId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetAuthorChatSessionsPaginatedQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetActorChatSessionsPaginatedQuery, PaginatedList<ChatSessionDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<PaginatedList<ChatSessionDto>> Handle(GetActorChatSessionsPaginatedQuery request, CancellationToken cancellationToken)
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