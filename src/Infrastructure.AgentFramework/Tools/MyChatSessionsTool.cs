using System.ComponentModel;
using Goodtocode.AgentFramework.Core.Application.Chats;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

public sealed class MyChatSessionsTool(IServiceProvider serviceProvider) : ScopedAgentTool(serviceProvider), IMyChatSessionsTool
{
    public static string ToolName => "MyChatSessionsTool";
    public string FunctionName => _currentFunctionName;
    public Dictionary<string, object> Parameters => _currentParameters;

    private string _currentFunctionName = string.Empty;
    private Dictionary<string, object> _currentParameters = [];

    [Description(
        """
        Lists chat sessions that belong to the current authenticated user.

        Use this tool whenever the user asks things like:
        - list my chat sessions
        - list my chats
        - show my chat history
        - show recent conversations
        - show my conversations
        - what conversations have I had
        - show previous chats
        - can you list any chat sessions I have

        Always call this tool for these requests. Do not answer from memory, do not say you lack
        access, and do not ask the user for permission before calling it - just call it.
        This tool already has access to the current user's context; no ownerId or tenantId is
        required from the user. Optionally filtered by startDate/endDate (defaults to the last 7 days).
        """)]
    public async Task<IEnumerable<string>> ListRecentSessionsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        _currentFunctionName = "list_sessions";
        _currentParameters = new()
        {
            { "startDate", startDate ?? DateTime.UtcNow.AddDays(-7) },
            { "endDate", endDate ?? DateTime.UtcNow.AddSeconds(1)}
        };

        var messages = await SendAsync(new GetMyChatSessionsQuery
        {
            StartDate = startDate,
            EndDate = endDate
        }, cancellationToken);

        return messages.Select(m => $"{m.Id}: {m.Timestamp} - {m.Title}");
    }

    [Description(
        """
        Renames/retitles an existing chat session for the current authenticated user.

        Use this tool whenever the user asks things like:
        - rename this chat session
        - change the title of chat session {id}
        - update chat session {id} title to {newTitle}

        Always call this tool for these requests instead of describing how to do it manually.
        Requires the chat session's id (sessionId) and the newTitle. This writes data, so call it
        only after the user explicitly confirms the newTitle and sessionId.
        """)]
    public async Task<string?> UpdateChatSessionTitleAsync(Guid sessionId, string newTitle, CancellationToken cancellationToken = default)
    {
        _currentFunctionName = "change_title";
        _currentParameters = new()
        {
            { "sessionId", sessionId },
            { "newTitle", newTitle }
        };

        var result = await SendAsync(new PatchMyChatSessionCommand
        {
            Id = sessionId,
            Title = newTitle
        }, cancellationToken);

        if (!result.IsSuccess)
        {
            return null;
        }

        var chatSession = await SendAsync(new GetMyChatSessionQuery
        {
            Id = sessionId
        }, cancellationToken);

        return chatSession is null
            ? null
            : $"{chatSession.Id}: {chatSession.Timestamp} - {chatSession.Title}\n[action|Review chat sessions|List my recent chat sessions]";
    }
}