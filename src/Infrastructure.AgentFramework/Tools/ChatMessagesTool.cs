using System.ComponentModel;
using Goodtocode.AgentFramework.Core.Application.Chat;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

public sealed class ChatMessagesTool(IServiceProvider serviceProvider) : ScopedAgentTool(serviceProvider), IChatMessagesTool
{
    public static string ToolName => "ChatMessagesTool";
    public string FunctionName => _currentFunctionName;
    public Dictionary<string, object> Parameters => _currentParameters;

    private string _currentFunctionName = string.Empty;
    private Dictionary<string, object> _currentParameters = [];

    [Description("Retrieves the most recent messages from all chat sessions.")]
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

    [Description("Retrieves all messages from a specific chat session.")]
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