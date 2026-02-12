using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Auth;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetMyChatSessionsQuery : IRequest<ICollection<ChatSessionDto>>, IRequiresUserContext
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public IUserContext? UserContext { get; set; }
}

public class GetMyChatSessionsQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyChatSessionsQuery, ICollection<ChatSessionDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ICollection<ChatSessionDto>> Handle(GetMyChatSessionsQuery request, CancellationToken cancellationToken)
    {
        GuardAgainstEmptyUser(request?.UserContext);

        var startDate = request?.StartDate;
        var endDate = request?.EndDate;
        var userContext = request?.UserContext;

        var returnData = await _context.ChatSessions
            .Where(x => userContext != null && x.OwnerId == userContext.OwnerId)
            .OrderByDescending(x => x.Timestamp)
            .Where(x => (startDate == null || x.Timestamp > startDate)
                    && (endDate == null || x.Timestamp < endDate))
            .Select(x => ChatSessionDto.CreateFrom(x))
            .ToListAsync(cancellationToken);

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