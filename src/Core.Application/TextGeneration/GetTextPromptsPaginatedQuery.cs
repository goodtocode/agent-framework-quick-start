using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Mappings;
using Goodtocode.AgentFramework.Core.Application.Common.Models;

namespace Goodtocode.AgentFramework.Core.Application.TextGeneration;

public class GetTextPromptsPaginatedQuery : IRequest<PaginatedList<TextPromptDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetTextPromptsPaginatedQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetTextPromptsPaginatedQuery, PaginatedList<TextPromptDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<PaginatedList<TextPromptDto>> Handle(GetTextPromptsPaginatedQuery request, CancellationToken cancellationToken)
    {
        var returnData = await _context.TextPrompts
            .OrderByDescending(x => x.Timestamp)
            .Where(x => (request.StartDate == null || x.Timestamp > request.StartDate)
                    && (request.EndDate == null || x.Timestamp < request.EndDate))
            .Select(x => TextPromptDto.CreateFrom(x))
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        return returnData;
    }
}