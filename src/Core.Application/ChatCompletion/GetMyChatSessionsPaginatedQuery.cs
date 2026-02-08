using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Application.Common.Mappings;
using Goodtocode.AgentFramework.Core.Application.Common.Models;
using Goodtocode.AgentFramework.Core.Domain.Auth;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetMyChatSessionsPaginatedQuery : IRequest<PaginatedList<ChatSessionDto>>, IRequiresUserContext
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public IUserContext? UserContext { get; set; }
}

public class GetMyChatSessionsPaginatedQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyChatSessionsPaginatedQuery, PaginatedList<ChatSessionDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<PaginatedList<ChatSessionDto>> Handle(GetMyChatSessionsPaginatedQuery request, CancellationToken cancellationToken)
    {
        GuardAgainstEmptyUser(request?.UserContext);
        var ownerId = request!.UserContext!.OwnerId;

        var returnData = await _context.ChatSessions
            .Include(x => x.Messages)
            .Where(x => x.OwnerId == ownerId)
            .OrderByDescending(x => x.Timestamp)
            .Where(x => (request.StartDate == null || x.Timestamp > request.StartDate)
                && (request.EndDate == null || x.Timestamp < request.EndDate))
            .Select(x => ChatSessionDto.CreateFrom(x))
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        return returnData;
    }

    private static void GuardAgainstEmptyUser(IUserContext? userContext)
    {
        if (userContext == null || userContext.OwnerId == Guid.Empty || userContext.TenantId == Guid.Empty)
            throw new CustomValidationException(
            [
                new("UserInfo", "User information is required to retrieve chat sessions")
            ]);
    }
}