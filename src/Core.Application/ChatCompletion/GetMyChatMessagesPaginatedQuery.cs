using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Mappings;
using Goodtocode.AgentFramework.Core.Application.Common.Models;
using Goodtocode.AgentFramework.Core.Domain.Auth;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetMyChatMessagesPaginatedQuery : IRequest<PaginatedList<ChatMessageDto>>, IRequiresUserContext
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public IUserContext? UserContext { get; set; }
}

public class GetChatMessagesPaginatedQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyChatMessagesPaginatedQuery, PaginatedList<ChatMessageDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<PaginatedList<ChatMessageDto>> Handle(GetMyChatMessagesPaginatedQuery request, CancellationToken cancellationToken)
    {
        GuardAgainstEmptyUser(request?.UserContext);

        var userContext =  request!.UserContext!;

        var returnData = await _context.ChatMessages
            .Where(x => x.ChatSession != null && x.ChatSession.OwnerId == userContext.OwnerId)
            .OrderByDescending(x => x.Timestamp)
            .Where(x => (request.StartDate == null || x.Timestamp > request.StartDate.Value)
                    && (request.EndDate == null || x.Timestamp < request.EndDate.Value))
            .Select(x => ChatMessageDto.CreateFrom(x))
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        return returnData;
    }

    private static void GuardAgainstEmptyUser(IUserContext? userContext)
    {
        if (userContext == null || userContext.OwnerId == Guid.Empty || userContext.TenantId == Guid.Empty)
            throw new CustomValidationException(
            [
                new("UserInfo", "User information is required to retrieve chat messages")
            ]);
    }
}