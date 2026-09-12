using Goodtocode.AgentFramework.Core.Domain.Chats;

namespace Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

/// <summary>
/// Resolves the customer's current position in the chat journey for a session: the suggested
/// prompts that start or continue the journey, and the follow-up action options parsed from the
/// latest assistant/system message so they always match the journey context.
/// </summary>
public class GetChatJourneyStepQuery : UserScopedRequest, IRequest<ChatJourneyStepDto>
{
    public Guid ChatSessionId { get; set; }
}

public class GetChatJourneyStepQueryHandler(
    IAgentFrameworkContext context,
    IChatJourneyProvider journeyProvider,
    IChatJourneyContextAccessor journeyContextAccessor,
    IChatSelectionTokenParser selectionTokenParser)
    : IRequestHandler<GetChatJourneyStepQuery, ChatJourneyStepDto>
{
    private readonly IAgentFrameworkContext _context = context;
    private readonly IChatJourneyProvider _journeyProvider = journeyProvider;
    private readonly IChatJourneyContextAccessor _journeyContextAccessor = journeyContextAccessor;
    private readonly IChatSelectionTokenParser _selectionTokenParser = selectionTokenParser;

    public async Task<ChatJourneyStepDto> Handle(GetChatJourneyStepQuery request, CancellationToken cancellationToken)
    {
        ChatGuard.GuardAgainstEmptyUserForQuery(request?.UserContext);
        ChatGuard.GuardAgainstEmptyId(request?.ChatSessionId);

        var chatSession = await _context.ChatSessions
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(
                x => x.Id == request!.ChatSessionId
                    && x.OwnerId == request.UserContext.OwnerId
                    && x.TenantId == request.UserContext.TenantId,
                cancellationToken);

        if (chatSession is null)
        {
            throw new CustomNotFoundException($"Chat session {request!.ChatSessionId:D} was not found.");
        }

        ChatGuard.GuardAgainstUnauthorized(chatSession, request!.UserContext!);

        var journeyContext = _journeyContextAccessor.GetContext(request.ChatSessionId);
        var suggestedPrompts = await _journeyProvider.ResolveSuggestedPromptsAsync(journeyContext, cancellationToken);
        var actionOptions = _selectionTokenParser.Parse(ResolveLatestRoutedContent(chatSession));

        return ChatJourneyStepDto.CreateFrom(
            new ChatJourneyStep(journeyContext.ResolveLevel(), suggestedPrompts, actionOptions));
    }

    private static string? ResolveLatestRoutedContent(ChatSessionEntity chatSession)
        => chatSession.Messages?
            .Where(message => message.Role is ChatMessageRole.assistant or ChatMessageRole.system)
            .OrderBy(message => message.Timestamp)
            .LastOrDefault()?
            .Content;
}
