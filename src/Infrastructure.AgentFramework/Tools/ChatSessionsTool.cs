using System.ComponentModel;
using Goodtocode.AgentFramework.Core.Application.Chat;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

public sealed class ChatSessionsTool(IServiceProvider serviceProvider) : ScopedAgentTool(serviceProvider), IChatSessionsTool
{
    public static string ToolName => "ChatSessionsTool";
    public string FunctionName => _currentFunctionName;
    public Dictionary<string, object> Parameters => _currentParameters;

    private string _currentFunctionName = string.Empty;
    private Dictionary<string, object> _currentParameters = [];

    [Description("Retrieves a list of recent chat sessions. Optionally, filter results by start and/or end date to narrow the search.")]
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

    [Description("Changes the title on this chat session.")]
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
            : $"{chatSession.Id}: {chatSession.Timestamp} - {chatSession.Title}";
    }
}