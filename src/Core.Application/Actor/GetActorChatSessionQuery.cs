using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.ChatCompletion;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;

namespace Goodtocode.AgentFramework.Core.Application.Actor;

public class GetActorChatSessionQuery : IRequest<ChatSessionDto>
{
    public Guid ActorId { get; set; }
    public Guid ChatSessionId { get; set; }
}

public class GetAuthorChatSessionQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetActorChatSessionQuery, ChatSessionDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ChatSessionDto> Handle(GetActorChatSessionQuery request, CancellationToken cancellationToken)
    {
        var returnData = await _context.ChatSessions
            .FirstOrDefaultAsync(x => x.Id == request.ChatSessionId && x.ActorId == request.ActorId, cancellationToken: cancellationToken);
        GuardAgainstNotFound(returnData);

        return ChatSessionDto.CreateFrom(returnData);
    }

    private static void GuardAgainstNotFound(ChatSessionEntity? entity)
    {
        if (entity is null)
            throw new CustomNotFoundException("Chat Session Not Found");
    }
}