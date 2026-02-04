using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;

public sealed class ChatMessagesTool(IServiceProvider serviceProvider) : AITool, IChatMessagesPlugin
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public static string PluginName => "ChatMessagesPlugin";
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

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IAgentFrameworkContext>();

        var query = context.ChatMessages.AsQueryable();
        if (startDate.HasValue) query = query.Where(x => x.Timestamp >= startDate.Value);
        if (endDate.HasValue) query = query.Where(x => x.Timestamp <= endDate.Value);

        var messages = await query.OrderByDescending(x => x.Timestamp).ToListAsync(cancellationToken);
        return messages.Select(m => $"{m.ChatSessionId}: {m.Timestamp:u} - {m.Role}: {m.Content}");
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

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IAgentFrameworkContext>();

        var messages = await context.ChatMessages
            .Where(x => x.ChatSessionId == sessionId)
            .ToListAsync(cancellationToken);

        return messages.Select(m => $"{m.ChatSessionId}: {m.Timestamp:u} - {m.Role}: {m.Content}");
    }
}