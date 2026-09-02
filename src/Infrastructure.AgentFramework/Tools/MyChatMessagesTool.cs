using System.ComponentModel;
using Goodtocode.AgentFramework.Core.Application.Chats;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

public sealed class MyChatMessagesTool(IServiceProvider serviceProvider) : ScopedAgentTool(serviceProvider), IMyChatMessagesTool
{
    public static string ToolName => "MyChatMessagesTool";
    public string FunctionName => _currentFunctionName;
    public Dictionary<string, object> Parameters => _currentParameters;

    private string _currentFunctionName = string.Empty;
    private Dictionary<string, object> _currentParameters = [];

    [Description(
        """
        Lists recent chat messages across all of the current authenticated user's chat sessions.

        Use this tool whenever the user asks things like:
        - show my recent messages
        - show recent messages across all my chat sessions
        - what have I said recently
        - show my message history

        Always call this tool for these requests. Do not answer from memory and do not claim you
        lack access - just call it. Optionally filtered by startDate/endDate (defaults to the last 7 days).
        """)]
    public async Task<IEnumerable<string>> ListRecentMessagesAsync(DateTime? startDate = null, DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        _currentFunctionName = "list_messages";
        _currentParameters = new()
        {
            { "startDate", startDate ?? DateTime.UtcNow.AddDays(-7) },
            { "endDate", endDate  ?? DateTime.UtcNow.AddSeconds(1)}
        };

        var messages = await SendAsync(new GetMyChatMessagesPaginatedQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            PageSize = 100
        }, cancellationToken);
        return messages.Items.Select(m => $"{m.ChatSessionId}: {m.Timestamp:u} - {m.Role}: {m.Content}");
    }

    [Description(
        """
        Lists all chat messages for one specific chat session (by sessionId) for the current
        authenticated user.

        Use this tool whenever the user asks things like:
        - show messages for chat session {id}
        - show the messages in this chat session
        - what did we talk about in chat session {id}

        Always call this tool for these requests instead of answering from memory.
        """)]
    public async Task<IEnumerable<string>> GetChatMessagesAsync(Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        _currentFunctionName = "get_messages";
        _currentParameters = new()
    {
        { "sessionId", sessionId }
    };

        var messages = await SendAsync(new GetMyChatSessionMessagesQuery
        {
            ChatSessionId = sessionId
        }, cancellationToken);

        return messages.Select(m => $"{m.ChatSessionId}: {m.Timestamp:u} - {m.Role}: {m.Content}");
    }
}