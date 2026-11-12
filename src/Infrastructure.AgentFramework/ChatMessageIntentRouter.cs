using System.Globalization;
using System.Text;
using Goodtocode.AgentFramework.Core.Application.Chats;
using Goodtocode.AgentFramework.Core.Application.Common.Auth;
using Goodtocode.AgentFramework.Core.Application.Governance;
using Goodtocode.AgentFramework.Core.Domain.Governance;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Governance;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;
using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;
using Goodtocode.Mediator;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework;

/// <summary>
/// Resolves chat replies through deterministic intent routing and purpose-specific inference.
/// All AI and chat-presentation behavior stays here so application
/// handlers depend only on <see cref="IChatMessageRouter"/>.
/// </summary>
public sealed class ChatMessageIntentRouter(
    AIAgent agent,
    [FromKeyedServices(InferencePurpose.Chat)] AIAgent chatAgent,
    ISender sender,
    IAgentFrameworkContext context,
    ChatGovernanceGate governanceGate,
    IRlsContext rlsContext,
    IWebSearchProvider webSearchProvider,
    IIntentClassifier intentClassifier,
    Tools.IAgentChatContextAccessor chatContextAccessor,
    IIntentPolicyProvider intentPolicyProvider,
    ChatGovernanceScope governanceScope,
    IAgentInstructionsComposer instructionsComposer,
    ILogger<ChatMessageIntentRouter> logger) : IChatMessageRouter, IIntentRouter
{
    private readonly IIntentPolicyProvider _intentPolicyProvider = intentPolicyProvider;
    private readonly ChatGovernanceScope _governanceScope = governanceScope;
    private readonly IAgentInstructionsComposer _instructionsComposer = instructionsComposer;
    private readonly AIAgent _agent = agent;
    private readonly AIAgent _chatAgent = chatAgent;
    private const string TextOnlyInstructions =
        "You are a text-only assistant. You have no application tools or access to live product data. " +
        "Answer from the conversation and general knowledge; do not claim to retrieve, change, or search application data.";
    private static readonly string[] ChatHistoryIntentPhrases =
        ["chat sessions", "chat session history", "chat history", "chat messages", "my conversations", "previous conversations"];
    private readonly ISender _sender = sender;
    private readonly IAgentFrameworkContext _context = context;
    private readonly ChatGovernanceGate _governanceGate = governanceGate;
    private readonly IRlsContext _rlsContext = rlsContext;
    private readonly IWebSearchProvider _webSearchProvider = webSearchProvider;
    private readonly IIntentClassifier _intentClassifier = intentClassifier;
    private readonly Tools.IAgentChatContextAccessor _chatContextAccessor = chatContextAccessor;
    private readonly ILogger<ChatMessageIntentRouter> _logger = logger;

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
            var match = await _intentClassifier.ClassifyAsync(message, priorUserMessages, cancellationToken);
            var deterministicReply = match is null ? null : await RouteAsync(chatSessionId, match, cancellationToken);
            if (!string.IsNullOrWhiteSpace(deterministicReply))
            {
                return deterministicReply;
            }
        }

        _chatContextAccessor.SetCurrentChatSession(chatSessionId);
        try
        {
            var governed = await GovernTurnAsync(chatSessionId, message, cancellationToken);
            var instruction = AgentInstruction.Create(
                $"{governed.PromptContext.SystemInstruction}\n\n{(mode == ChatRoutingMode.Direct ? TextOnlyInstructions : _instructionsComposer.Compose())}",
                InstructionScope.Session);
            using var governedScope = _governanceScope.Begin(_rlsContext.TenantId, instruction);
            var chatHistory = await BuildChatHistoryAsync(chatSessionId, message, cancellationToken);

            if (mode == ChatRoutingMode.Direct)
            {
                chatHistory.Add(new(ChatRole.System, TextOnlyInstructions));
            }

            var requiresTool = mode == ChatRoutingMode.Routed
                && ChatHistoryIntentPhrases.Any(phrase => message.Contains(phrase, StringComparison.OrdinalIgnoreCase));
            var inferenceAgent = mode == ChatRoutingMode.Direct ? _chatAgent : _agent;
            var runOptions = requiresTool
                ? new ChatClientAgentRunOptions(new ChatOptions { ToolMode = ChatToolMode.RequireAny })
                : null;
            var agentResponse = await inferenceAgent.RunAsync(chatHistory, options: runOptions, cancellationToken: cancellationToken);
            if (requiresTool && !agentResponse.Messages.Any(item => item.Contents.OfType<FunctionCallContent>().Any()))
            {
                throw new InvalidOperationException("Required-tool inference completed without a function call.");
            }

            var response = agentResponse.Messages.LastOrDefault();
            ChatGuard.GuardAgainstNullAgentResponse(response);
            var reply = response!.Contents.LastOrDefault()?.ToString() ?? string.Empty;
            if (requiresTool && string.IsNullOrWhiteSpace(reply))
            {
                throw new InvalidOperationException("Required-tool inference completed without a reply.");
            }
            return await GuardReplyAsync(reply, chatHistory, governed.PromptContext.SystemInstruction, cancellationToken);
        }
        finally
        {
            _chatContextAccessor.ClearCurrentChatSession();
        }
    }

    /// <summary>
    /// Single exit validation for every model-generated reply. A reply containing future tense or an
    /// announced intent is regenerated once with a corrective instruction; whatever still violates is
    /// stripped, so such text never reaches the user.
    /// </summary>
    private async Task<string> GuardReplyAsync(
        string reply,
        List<ChatMessage> chatHistory,
        string governedInstruction,
        CancellationToken cancellationToken)
    {
        if (!FutureTenseGuard.ContainsFutureTense(reply))
        {
            return reply;
        }

        _logger.LogWarning("Model reply contained future tense or an announced intent; regenerating once.");
        try
        {
            var retryHistory = new List<ChatMessage>(chatHistory)
            {
                new(ChatRole.Assistant, reply),
                new(ChatRole.System, TextOnlyInstructions),
                new(ChatRole.System, ToolRoutingInstructions.NoFutureTensePreamble),
                new(ChatRole.System, "The previous reply violated the rule above. Produce the final answer now, using only past or present tense and containing results rather than announcements.")
            };
            var retryInstruction = AgentInstruction.Create($"{governedInstruction}\n\n{TextOnlyInstructions}", InstructionScope.Session);
            using var retryScope = _governanceScope.Begin(_rlsContext.TenantId, retryInstruction);
            var retryResponse = await _chatAgent.RunAsync(retryHistory, cancellationToken: cancellationToken);
            var retryReply = retryResponse.Messages.LastOrDefault()?.Contents.LastOrDefault()?.ToString();
            if (!string.IsNullOrWhiteSpace(retryReply))
            {
                return FutureTenseGuard.Enforce(retryReply);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(exception, "Regeneration after a future-tense reply failed; stripping the reply instead.");
        }

        return FutureTenseGuard.Enforce(reply);
    }

    private async Task<GovernedEvaluationResult> GovernTurnAsync(
        Guid chatSessionId,
        string userMessage,
        CancellationToken cancellationToken)
    {
        var policy = await _intentPolicyProvider.ResolveAsync(new IntentPolicyRequest(_rlsContext.TenantId), cancellationToken);
        var governed = _governanceGate.Enforce(_rlsContext, userMessage, policy);
        _context.Set<ChatGovernanceEntity>().Add(_governanceGate.CreatePersistenceRecord(
            _rlsContext.OwnerId,
            _rlsContext.TenantId,
            chatSessionId,
            governed));
        await _context.SaveChangesAsync(cancellationToken);
        return governed;
    }

    private async Task<List<ChatMessage>> BuildChatHistoryAsync(
        Guid chatSessionId,
        string userMessage,
        CancellationToken cancellationToken)
    {
        var chatSession = await _sender.Send(new GetMyChatSessionQuery { Id = chatSessionId }, cancellationToken);
        var chatHistory = new List<ChatMessage>
        {
            new(ChatRole.System, ToolRoutingInstructions.NoFutureTensePreamble),
            BuildSelectionContextMessage(chatSessionId)
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

    private ChatMessage BuildSelectionContextMessage(Guid chatSessionId)
    {
        var selection = _chatContextAccessor.GetOrCreateContext(chatSessionId);
        var content = $"""
            Current chat selection context:
            - Selected ActorId: {selection.ActorId?.ToString("D") ?? "none"}
            - Selected ChatSessionId: {selection.SelectedChatSessionId?.ToString("D") ?? "none"}
            """;

        return new ChatMessage(ChatRole.System, content);
    }

    /// <inheritdoc />
    public Task<string> RouteAsync(Guid chatSessionId, IntentMatch match, CancellationToken cancellationToken) => match.Intent.Name switch
    {
        IntentNames.QueryChatSessionsList => QueryChatSessionsListAsync(chatSessionId, cancellationToken),
        IntentNames.QueryMyChatMessagesForCurrentChatSession => QueryMyChatMessagesForCurrentChatSessionAsync(chatSessionId, cancellationToken),
        IntentNames.QueryChatMessagesList => QueryChatMessagesListAsync(cancellationToken),
        IntentNames.QueryChatMessagesForSession => QueryChatMessagesForSessionAsync(Guid.Parse(match.Captures!["sessionId"]), cancellationToken),
        IntentNames.QueryActorById => QueryActorByIdAsync(chatSessionId, Guid.Parse(match.Captures!["id"]), cancellationToken),
        IntentNames.QueryActorsByName => QueryActorsByNameAsync(chatSessionId, match, cancellationToken),
        IntentNames.QueryActorsList => QueryActorsListAsync(chatSessionId, cancellationToken),
        IntentNames.QueryMyActorsList => QueryMyActorsListAsync(chatSessionId, cancellationToken),
        IntentNames.SelectActor => SelectActorAsync(chatSessionId, Guid.Parse(match.Captures!["id"]), cancellationToken),
        IntentNames.SelectChatSession => SelectChatSessionAsync(chatSessionId, Guid.Parse(match.Captures!["id"]), cancellationToken),
        IntentNames.QueryChatSessionsForSelectedActor => QueryChatSessionsForSelectedActorAsync(chatSessionId, cancellationToken),
        IntentNames.QueryChatMessagesForSelectedChatSession => QueryChatMessagesForSelectedChatSessionAsync(chatSessionId, cancellationToken),
        IntentNames.SearchWeb => QueryWebSearchAsync(match.Captures!["query"], cancellationToken),
        _ => throw new InvalidOperationException($"No route registered for intent '{match.Intent.Name}'.")
    };

    /// <summary>
    /// Mid-chain journey entry point: lists the current user's own chat sessions without first
    /// requiring an actor lookup/selection. The actor is implied by the authenticated user's
    /// OwnerId, so the journey context is advanced to that actor and chat-session selection chips
    /// are emitted, letting the customer continue straight into the standard aggregate chain.
    /// </summary>
    private async Task<string> QueryChatSessionsListAsync(Guid chatSessionId, CancellationToken cancellationToken)
    {
        var sessions = (await _sender.Send(new GetMyChatSessionsQuery(), cancellationToken))
            .OrderByDescending(session => session.Timestamp)
            .Take(10)
            .ToList();
        if (sessions.Count == 0)
        {
            return "You have no chat sessions yet.";
        }

        var markdown = new StringBuilder(MarkdownTableFormatter.Format(
            ["#", "Title", "Chat Session Id", "Timestamp (UTC)"],
            sessions.Select((session, index) => (IReadOnlyList<string?>)[
                (index + 1).ToString(CultureInfo.InvariantCulture),
                session.Title,
                $"`{session.Id:D}`",
                session.Timestamp.ToString("u", CultureInfo.InvariantCulture)])));

        markdown.AppendLine();
        markdown.AppendLine("Select a chat session:");
        foreach (var session in sessions)
        {
            markdown.AppendLine($"[selection|chatsession|{session.Id:D}||{EscapeSelectionLabel(session.Title)}]");
        }

        var currentActor = await _sender.Send(new Core.Application.Actors.GetMyActorQuery(), cancellationToken);
        _chatContextAccessor.UpsertContext(chatSessionId, current => current with
        {
            ActorId = currentActor?.Id ?? current.ActorId,
            MyChatSessionsListed = true,
            SelectedChatSessionId = null
        });

        return markdown.ToString();
    }

    /// <summary>
    /// Mid-chain journey entry point: lists the current user's messages for the chat session that is
    /// currently selected, falling back to the conversation the customer is already in. Scoping stays
    /// on the authenticated user's OwnerId/TenantId because the underlying query is a My request.
    /// </summary>
    private async Task<string> QueryMyChatMessagesForCurrentChatSessionAsync(Guid chatSessionId, CancellationToken cancellationToken)
    {
        var context = _chatContextAccessor.GetOrCreateContext(chatSessionId);
        var targetChatSessionId = context.SelectedChatSessionId ?? chatSessionId;

        _chatContextAccessor.UpsertContext(chatSessionId, current => current with
        {
            SelectedChatSessionId = targetChatSessionId
        });

        return await QueryChatMessagesForSessionAsync(targetChatSessionId, cancellationToken);
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

    private async Task<string> QueryActorByIdAsync(Guid chatSessionId, Guid actorId, CancellationToken cancellationToken)
    {
        var actor = await _sender.Send(new Core.Application.Actors.GetOurActorQuery { ActorId = actorId }, cancellationToken);
        if (actor is null)
        {
            return $"No actor was found with id `{actorId:D}`.";
        }

        _chatContextAccessor.UpsertContext(chatSessionId, current => current with { ActorId = actor.Id });
        return $"Actor `{actor.Id:D}`: {actor.FirstName} {actor.LastName}".TrimEnd();
    }

    private async Task<string> QueryActorsByNameAsync(Guid chatSessionId, IntentMatch match, CancellationToken cancellationToken)
    {
        if (match.Captures is null)
        {
            return "Please provide the name of the actor you want to find.";
        }

        var name = match.Captures.TryGetValue("name", out var capturedName)
            ? capturedName
            : match.Captures["followUp"];
        var actors = (await _sender.Send(new Core.Application.Actors.GetOurActorsByNameQuery
        {
            Name = name
        }, cancellationToken)).ToList();
        if (actors.Count == 0)
        {
            return $"No actors were found matching \"{EscapeCell(name)}\".";
        }

        return BuildActorSelectionMarkdown(chatSessionId, actors);
    }

    private async Task<string> QueryChatMessagesForSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var messages = await _sender.Send(new GetMyChatSessionMessagesQuery
        {
            ChatSessionId = sessionId
        }, cancellationToken);

        var items = messages.ToList();
        if (items.Count == 0)
        {
            return "No messages were found for this chat session.";
        }

        return MarkdownTableFormatter.Format(
            ["#", "Chat Session Id", "Timestamp (UTC)", "Role", "Content"],
            items.Select((message, index) => (IReadOnlyList<string?>)[
                (index + 1).ToString(CultureInfo.InvariantCulture),
                $"`{message.ChatSessionId:D}`",
                message.Timestamp.ToString("u", CultureInfo.InvariantCulture),
                message.Role,
                message.Content]));
    }

    private async Task<string> QueryActorsListAsync(Guid chatSessionId, CancellationToken cancellationToken)
    {
        var actors = (await _sender.Send(new Core.Application.Actors.GetOurActorsQuery(), cancellationToken)).ToList();
        return BuildActorSelectionMarkdown(chatSessionId, actors);
    }

    private async Task<string> QueryMyActorsListAsync(Guid chatSessionId, CancellationToken cancellationToken)
    {
        var actors = (await _sender.Send(new Core.Application.Actors.GetMyActorsQuery(), cancellationToken)).ToList();
        return BuildActorSelectionMarkdown(chatSessionId, actors);
    }

    private string BuildActorSelectionMarkdown(Guid chatSessionId, IReadOnlyList<Core.Application.Actors.ActorDto> actors)
    {
        if (actors.Count == 0)
        {
            return "No actors were found.";
        }

        var ordered = actors.OrderByDescending(actor => actor.CreatedOn).ToList();
        var markdown = new StringBuilder(MarkdownTableFormatter.Format(
            ["#", "Actor ID", "Name", "Timestamp (UTC)"],
            ordered.Select((actor, index) => (IReadOnlyList<string?>)[
                (index + 1).ToString(CultureInfo.InvariantCulture),
                $"`{actor.Id:D}`",
                $"{actor.FirstName} {actor.LastName}".Trim(),
                actor.CreatedOn.ToString("u", CultureInfo.InvariantCulture)])));

        markdown.AppendLine();
        markdown.AppendLine("Select an actor:");
        foreach (var actor in ordered)
        {
            var label = $"{actor.FirstName} {actor.LastName}".Trim();
            markdown.AppendLine($"[selection|actor|{actor.Id:D}||{EscapeSelectionLabel(string.IsNullOrWhiteSpace(label) ? actor.Id.ToString("D") : label)}]");
        }

        _chatContextAccessor.UpsertContext(chatSessionId, context => context with
        {
            ActorId = ordered[0].Id,
            MyChatSessionsListed = false,
            SelectedChatSessionId = null
        });

        return markdown.ToString();
    }

    private async Task<string> SelectActorAsync(Guid chatSessionId, Guid actorId, CancellationToken cancellationToken)
    {
        var actor = await _sender.Send(new Core.Application.Actors.GetOurActorQuery { ActorId = actorId }, cancellationToken);
        if (actor is null)
        {
            return $"No actor was found with id `{actorId:D}`.";
        }

        _chatContextAccessor.UpsertContext(chatSessionId, current => current with
        {
            ActorId = actor.Id,
            MyChatSessionsListed = false,
            SelectedChatSessionId = null
        });

        var markdown = new StringBuilder();
        var name = $"{actor.FirstName} {actor.LastName}".Trim();
        markdown.AppendLine($"Selected actor: **{name}** (`{actor.Id:D}`)");
        markdown.AppendLine();
        markdown.AppendLine(await QueryChatSessionsForSelectedActorAsync(chatSessionId, cancellationToken));
        return markdown.ToString();
    }

    private async Task<string> QueryChatSessionsForSelectedActorAsync(Guid chatSessionId, CancellationToken cancellationToken)
    {
        var context = _chatContextAccessor.GetOrCreateContext(chatSessionId);
        if (!context.ActorId.HasValue)
        {
            return "No actor is selected. First run: Query our actor list.";
        }

        var sessions = (await _sender.Send(new Core.Application.Actors.GetOurActorChatSessionsQuery
        {
            ActorId = context.ActorId.Value
        }, cancellationToken)).OrderByDescending(session => session.Timestamp).Take(10).ToList();

        if (sessions.Count == 0)
        {
            return $"No chat sessions were found for actor `{context.ActorId.Value:D}`.";
        }

        var markdown = new StringBuilder(MarkdownTableFormatter.Format(
            ["#", "Title", "Chat Session Id", "Timestamp (UTC)"],
            sessions.Select((session, index) => (IReadOnlyList<string?>)[
                (index + 1).ToString(CultureInfo.InvariantCulture),
                session.Title,
                $"`{session.Id:D}`",
                session.Timestamp.ToString("u", CultureInfo.InvariantCulture)])));

        markdown.AppendLine();
        markdown.AppendLine("Select a chat session:");
        foreach (var session in sessions)
        {
            markdown.AppendLine($"[selection|chatsession|{session.Id:D}||{EscapeSelectionLabel(session.Title)}]");
        }

        _chatContextAccessor.UpsertContext(chatSessionId, current => current with { SelectedChatSessionId = sessions[0].Id });
        return markdown.ToString();
    }

    private async Task<string> SelectChatSessionAsync(Guid chatSessionId, Guid selectedChatSessionId, CancellationToken cancellationToken)
    {
        _chatContextAccessor.UpsertContext(chatSessionId, current => current with { SelectedChatSessionId = selectedChatSessionId });
        return await QueryChatMessagesForSelectedChatSessionAsync(chatSessionId, cancellationToken);
    }

    private async Task<string> QueryChatMessagesForSelectedChatSessionAsync(Guid chatSessionId, CancellationToken cancellationToken)
    {
        var context = _chatContextAccessor.GetOrCreateContext(chatSessionId);
        if (!context.SelectedChatSessionId.HasValue)
        {
            return "No chat session is selected. First run: Query chat sessions for the selected actor.";
        }

        return await QueryChatMessagesForSessionAsync(context.SelectedChatSessionId.Value, cancellationToken);
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

    private static string EscapeSelectionLabel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Replace("]", ")", StringComparison.Ordinal).Replace("|", "/", StringComparison.Ordinal);
    }
}