using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Mappings;
using Goodtocode.AgentFramework.Core.Application.Common.Models;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetChatSessionsPaginatedQuery : IRequest<PaginatedList<ChatSessionDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetChatSessionsPaginatedQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetChatSessionsPaginatedQuery, PaginatedList<ChatSessionDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<PaginatedList<ChatSessionDto>> Handle(GetChatSessionsPaginatedQuery request, CancellationToken cancellationToken)
    {
        var returnData = await _context.ChatSessions
            .OrderByDescending(x => x.Timestamp)
            .Where(x => (request.StartDate == null || x.Timestamp > request.StartDate)
                    && (request.EndDate == null || x.Timestamp < request.EndDate))
            .Select(x => ChatSessionDto.CreateFrom(x))
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        return returnData;
    }
}