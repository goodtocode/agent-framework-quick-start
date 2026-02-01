using Goodtocode.AgentFramework.Core.Application.Abstractions;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetChatMessagesQuery : IRequest<ICollection<ChatMessageDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetChatMessagesQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetChatMessagesQuery, ICollection<ChatMessageDto>>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ICollection<ChatMessageDto>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var returnData = await _context.ChatMessages
            .OrderByDescending(x => x.Timestamp)
            .Where(x => (request.StartDate == null || x.Timestamp > request.StartDate)
                    && (request.EndDate == null || x.Timestamp < request.EndDate))
            .Select(x => ChatMessageDto.CreateFrom(x))
            .ToListAsync(cancellationToken);

        return returnData;
    }
}