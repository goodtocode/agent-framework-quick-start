using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Auth;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetMyChatSessionQuery : IRequest<ChatSessionDto>, IUserInfoRequest
{
    public Guid ChatSessionId { get; set; }
    public IUserEntity? UserInfo { get; set; }
}

public class GetAuthorChatSessionByOwnerIdQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetMyChatSessionQuery, ChatSessionDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ChatSessionDto> Handle(GetMyChatSessionQuery request, CancellationToken cancellationToken)
    {
        var returnData = await _context.ChatSessions
            .Where(cs => cs.Id == request.ChatSessionId)
            .Join(_context.Actors,
                  cs => cs.ActorId,
                  a => a.Id,
                  (cs, a) => new { ChatSession = cs, Actor = a })
            .Where(joined => joined.Actor.OwnerId == request.UserInfo!.OwnerId)
            .Select(joined => joined.ChatSession)
            .FirstOrDefaultAsync(cancellationToken);

        GuardAgainstNotFound(returnData);

        return ChatSessionDto.CreateFrom(returnData);
    }

    private static void GuardAgainstNotFound(ChatSessionEntity? entity)
    {
        if (entity is null)
            throw new CustomNotFoundException("Chat Session Not Found");
    }
}