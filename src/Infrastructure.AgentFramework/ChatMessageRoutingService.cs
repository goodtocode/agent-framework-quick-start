using System.Text;
using System.Globalization;
using Goodtocode.AgentFramework.Core.Application.Common.Auth;
using Goodtocode.AgentFramework.Core.Application.Chats;
using Goodtocode.AgentFramework.Core.Application.Governance;
using Goodtocode.AgentFramework.Core.Domain.Governance;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;
using Goodtocode.Mediator;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework;

/// <summary>
/// Resolves chat replies through deterministic intent routing, forced-tool inference, and finally
/// the normal open agent turn. All AI and chat-presentation behavior stays here so application
/// handlers depend only on <see cref="IChatMessageRoutingService"/>.
/// </summary>
public sealed class ChatMessageRoutingService(
    AIAgent agent,
    ISender sender,
    IAgentFrameworkContext context,
    ChatGovernanceGate governanceGate,
    IRlsContext rlsContext,
    IWebSearchProvider webSearchProvider,
    IIntentClassifier intentClassifier,
    ILogger<ChatMessageRoutingService> logger) : IChatMessageRoutingService, IIntentRouter
{
    private readonly AIAgent _agent = agent;
    private readonly ISender _sender = sender;
    private readonly IAgentFrameworkContext _context = context;
    private readonly ChatGovernanceGate _governanceGate = governanceGate;
    private readonly IRlsContext _rlsContext = rlsContext;
    private readonly IWebSearchProvider _webSearchProvider = webSearchProvider;
    private readonly IIntentClassifier _intentClassifier = intentClassifier;
    private readonly ILogger<ChatMessageRoutingService> _logger = logger;
    private static readonly Action<ILogger, Exception?> LogForcedToolInferenceFailure = LoggerMessage.Define(
        LogLevel.Warning,
        new EventId(1, nameof(LogForcedToolInferenceFailure)),
        "Forced-tool inference (tier 2) failed; falling back to an open agent turn.");

    /// <inheritdoc />
    public async Task<string> ResolveReplyAsync(
        Guid chatSessionId,
        string message,
        CancellationToken cancellationToken,
        ChatRoutingMode mode = ChatRoutingMode.Routed)
    {
        if (mode == ChatRoutingMode.Routed)
        {
            var match = _intentClassifier.Classify(message);
            var deterministicReply = match is null
                ? null
                : await RouteAsync(chatSessionId, match, cancellationToken);
            if (!string.IsNullOrWhiteSpace(deterministicReply))
            {
                return deterministicReply;
            }
        }

        var chatHistory = await BuildChatHistoryAsync(chatSessionId, message, cancellationToken);

        if (mode == ChatRoutingMode.Routed)
        {
            var forcedToolReply = await TryResolveViaForcedToolInferenceAsync(chatHistory, cancellationToken);
            if (!string.IsNullOrWhiteSpace(forcedToolReply))
            {
                return forcedToolReply;
            }
        }

        var agentResponse = await _agent.RunAsync(chatHistory, cancellationToken: cancellationToken);
        var response = agentResponse.Messages.LastOrDefault();
        ChatGuard.GuardAgainstNullAgentResponse(response);
        return response!.Contents.LastOrDefault()?.ToString() ?? string.Empty;
    }

    private async Task<string?> TryResolveViaForcedToolInferenceAsync(
        List<ChatMessage> chatHistory,
        CancellationToken cancellationToken)
    {
        try
        {
            var runOptions = new ChatClientAgentRunOptions(new ChatOptions { ToolMode = ChatToolMode.RequireAny });
            var agentResponse = await _agent.RunAsync(chatHistory, options: runOptions, cancellationToken: cancellationToken);
            return agentResponse.Messages.LastOrDefault()?.Contents.LastOrDefault()?.ToString();
        }
        catch (Exception exception)
        {
            LogForcedToolInferenceFailure(_logger, exception);
            return null;
        }
    }

    private async Task<List<ChatMessage>> BuildChatHistoryAsync(
        Guid chatSessionId,
        string userMessage,
        CancellationToken cancellationToken)
    {
        var chatSession = await _sender.Send(new GetMyChatSessionQuery { Id = chatSessionId }, cancellationToken);
        var governed = _governanceGate.Enforce(_rlsContext, userMessage);
        _context.Set<ChatGovernanceEntity>().Add(_governanceGate.CreatePersistenceRecord(
            _rlsContext.OwnerId,
            _rlsContext.TenantId,
            chatSessionId,
            governed));
        await _context.SaveChangesAsync(cancellationToken);
        var chatHistory = new List<ChatMessage>
        {
            new(ChatRole.System, governed.PromptContext.SystemInstruction)
        };

        foreach (var message in chatSession?.Messages ?? [])
        {
            chatHistory.Add(new ChatMessage(
                message.Role.Equals("user", StringComparison.OrdinalIgnoreCase) ? ChatRole.User : ChatRole.Assistant,
                message.Content));
        }

        chatHistory.Add(new ChatMessage(ChatRole.User, userMessage));
        return chatHistory;
    }

    /// <inheritdoc />
    public Task<string> RouteAsync(Guid chatSessionId, IntentMatch match, CancellationToken cancellationToken) => match.Intent.Name switch
    {
        IntentNames.QueryChatSessionsList => QueryChatSessionsListAsync(cancellationToken),
        IntentNames.QueryChatMessagesList => QueryChatMessagesListAsync(cancellationToken),
        IntentNames.QueryActorById => QueryActorByIdAsync(Guid.Parse(match.Captures!["id"]), cancellationToken),
        IntentNames.SearchWeb => QueryWebSearchAsync(match.Captures!["query"], cancellationToken),
        _ => throw new InvalidOperationException($"No route registered for intent '{match.Intent.Name}'.")
    };

    private async Task<string> QueryChatSessionsListAsync(CancellationToken cancellationToken)
    {
        var sessions = (await _sender.Send(new GetMyChatSessionsQuery(), cancellationToken))
            .OrderByDescending(session => session.Timestamp)
            .Take(10)
            .ToList();
        if (sessions.Count == 0)
        {
            return "You have no chat sessions yet.";
        }

        var reply = new StringBuilder("| # | Title | Chat Session Id | Timestamp (UTC) |\n|---|---|---|---|\n");
        for (var index = 0; index < sessions.Count; index++)
        {
            var session = sessions[index];
            reply.AppendLine(CultureInfo.InvariantCulture, $"| {index + 1} | {EscapeCell(session.Title)} | `{session.Id:D}` | {session.Timestamp:u} |");
        }

        return reply.ToString();
    }

    private async Task<string> QueryChatMessagesListAsync(CancellationToken cancellationToken)
    {
        var messages = await _sender.Send(new GetMyChatMessagesPaginatedQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow.AddSeconds(1),
            PageSize = 10
        }, cancellationToken);
        if (messages.Items.Count == 0)
        {
            return "You have no recent chat messages in the last 7 days.";
        }

        var reply = new StringBuilder("| # | Chat Session Id | Timestamp (UTC) | Role | Content |\n|---|---|---|---|---|\n");
        foreach (var message in messages.Items.Select((message, index) => new { Message = message, Index = index }))
        {
            reply.AppendLine(CultureInfo.InvariantCulture, $"| {message.Index + 1} | `{message.Message.ChatSessionId:D}` | {message.Message.Timestamp:u} | {message.Message.Role} | {EscapeCell(message.Message.Content)} |");
        }

        return reply.ToString();
    }

    private async Task<string> QueryActorByIdAsync(Guid actorId, CancellationToken cancellationToken)
    {
        var actor = await _sender.Send(new Core.Application.Actors.GetOurActorQuery { ActorId = actorId }, cancellationToken);
        return actor is null
            ? $"No actor was found with id `{actorId:D}`."
            : $"Actor `{actor.Id:D}`: {actor.FirstName} {actor.LastName}".TrimEnd();
    }

    private async Task<string> QueryWebSearchAsync(string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return "Please provide a web search query.";
        }

        var result = await _webSearchProvider.SearchAsync(query.Trim(), cancellationToken);
        if (result.Results.Count == 0)
        {
            return $"No web search results were found for \"{query}\".";
        }

        var reply = new StringBuilder($"Web search results for \"{query}\":\n\n| # | Title | Snippet | Url |\n|---|---|---|---|\n");
        for (var index = 0; index < result.Results.Count; index++)
        {
            var item = result.Results[index];
            reply.AppendLine(CultureInfo.InvariantCulture, $"| {index + 1} | {EscapeCell(item.Title)} | {EscapeCell(item.Snippet)} | {item.Url} |");
        }

        return reply.ToString();
    }

    private static string EscapeCell(string? value) => (value ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
}