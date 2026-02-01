using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;

namespace Goodtocode.AgentFramework.Core.Application.ChatCompletion;

public class GetChatMessageQuery : IRequest<ChatMessageDto>
{
    public Guid Id { get; set; }
}

public class GetChatMessageQueryHandler(IAgentFrameworkContext context) : IRequestHandler<GetChatMessageQuery, ChatMessageDto>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<ChatMessageDto> Handle(GetChatMessageQuery request,
                                CancellationToken cancellationToken)
    {
        var chatMessage = await _context.ChatMessages.FindAsync([request.Id, cancellationToken], cancellationToken: cancellationToken);
        GuardAgainstNotFound(chatMessage);

        return ChatMessageDto.CreateFrom(chatMessage);
    }

    private static void GuardAgainstNotFound(ChatMessageEntity? chatMessage)
    {
        if (chatMessage == null)
            throw new CustomNotFoundException("Chat Message Not Found");
    }
}