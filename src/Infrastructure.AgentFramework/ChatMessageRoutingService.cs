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
            var session = await _sender.Send(new GetMyChatSessionQuery { Id = chatSessionId }, cancellationToken);
            var priorUserMessages = session?.Messages?
                .Where(x => x.Role.Equals("user", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Content)
                .ToList();
            var match = _intentClassifier.Classify(message, priorUserMessages);
            var deterministicReply = match is null ? null : await RouteAsync(chatSessionId, match, cancellationToken);
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
        IntentNames.QueryActorsByName => QueryActorsByNameAsync(match, cancellationToken),
        IntentNames.QueryActorsList => QueryActorsListAsync(cancellationToken),
        IntentNames.QueryMyActorsList => QueryMyActorsListAsync(cancellationToken),
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

        return MarkdownTableFormatter.Format(
            ["#", "Title", "Chat Session Id", "Timestamp (UTC)"],
            sessions.Select((session, index) => (IReadOnlyList<string?>)[
                (index + 1).ToString(CultureInfo.InvariantCulture),
                session.Title,
                $"`{session.Id:D}`",
                session.Timestamp.ToString("u", CultureInfo.InvariantCulture)]));
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

        return MarkdownTableFormatter.Format(
            ["#", "Chat Session Id", "Timestamp (UTC)", "Role", "Content"],
            messages.Items.Select((message, index) => (IReadOnlyList<string?>)[
                (index + 1).ToString(CultureInfo.InvariantCulture),
                $"`{message.ChatSessionId:D}`",
                message.Timestamp.ToString("u", CultureInfo.InvariantCulture),
                message.Role,
                message.Content]));
    }

    private async Task<string> QueryActorByIdAsync(Guid actorId, CancellationToken cancellationToken)
    {
        var actor = await _sender.Send(new Core.Application.Actors.GetOurActorQuery { ActorId = actorId }, cancellationToken);
        return actor is null
            ? $"No actor was found with id `{actorId:D}`."
            : $"Actor `{actor.Id:D}`: {actor.FirstName} {actor.LastName}".TrimEnd();
    }

    private async Task<string> QueryActorsByNameAsync(IntentMatch match, CancellationToken cancellationToken)
    {
        if (match.Captures is null)
        {
            return "Please provide the name of the actor you want to find.";
        }

        var name = match.Captures.TryGetValue("name", out var capturedName)
            ? capturedName
            : match.Captures["followUp"];
        var actors = await _sender.Send(new Core.Application.Actors.GetOurActorsByNameQuery
        {
            Name = name
        }, cancellationToken);
        if (actors.Count == 0)
        {
            return $"No actors were found matching \"{EscapeCell(name)}\".";
        }

        return MarkdownTableFormatter.Format(
            ["#", "Actor ID", "Name", "Timestamp (UTC)"],
            actors.Select((actor, index) => (IReadOnlyList<string?>)[
                (index + 1).ToString(CultureInfo.InvariantCulture),
                $"`{actor.Id:D}`",
                $"{actor.FirstName} {actor.LastName}".Trim(),
                actor.CreatedOn.ToString("u", CultureInfo.InvariantCulture)]));
    }

    private async Task<string> QueryActorsListAsync(CancellationToken cancellationToken)
    {
        var actors = await _sender.Send(new Core.Application.Actors.GetOurActorsQuery(), cancellationToken);
        return FormatActors(actors);
    }

    private async Task<string> QueryMyActorsListAsync(CancellationToken cancellationToken)
    {
        var actors = await _sender.Send(new Core.Application.Actors.GetMyActorsQuery(), cancellationToken);
        return FormatActors(actors);
    }

    private static string FormatActors(ICollection<Core.Application.Actors.ActorDto> actors)
    {
        if (actors.Count == 0)
        {
            return "No actors were found.";
        }

        return MarkdownTableFormatter.Format(
            ["#", "Actor ID", "Name", "Timestamp (UTC)"],
            actors.Select((actor, index) => (IReadOnlyList<string?>)[
                (index + 1).ToString(CultureInfo.InvariantCulture),
                $"`{actor.Id:D}`",
                $"{actor.FirstName} {actor.LastName}".Trim(),
                actor.CreatedOn.ToString("u", CultureInfo.InvariantCulture)]));
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

        var rows = result.Results.Select((item, index) => (IReadOnlyList<string?>)[
            (index + 1).ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.Snippet,
            item.Url]);
        return $"Web search results for \"{MarkdownTableFormatter.EscapeCell(query)}\":\n\n" +
            MarkdownTableFormatter.Format(["#", "Title", "Snippet", "Url"], rows);
    }

    private static string EscapeCell(string? value) => MarkdownTableFormatter.EscapeCell(value);
}