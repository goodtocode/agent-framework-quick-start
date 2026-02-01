using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Mappings;
using Goodtocode.AgentFramework.Core.Application.Common.Models;

namespace Goodtocode.AgentFramework.Core.Application.Image;

public class GetTextImagesPaginatedQuery : IRequest<PaginatedList<TextImageDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetTextImagesPaginatedQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetTextImagesPaginatedQuery, PaginatedList<TextImageDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<PaginatedList<TextImageDto>> Handle(GetTextImagesPaginatedQuery request, CancellationToken cancellationToken)
    {
        var returnData = await _context.TextImages
            .OrderByDescending(x => x.Timestamp)
            .Where(x => (request.StartDate == null || x.Timestamp > request.StartDate)
                    && (request.EndDate == null || x.Timestamp < request.EndDate))
            .Select(x => TextImageDto.CreateFrom(x))
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        return returnData;
    }
}