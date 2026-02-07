namespace Goodtocode.AgentFramework.Core.Application.Abstractions;

public interface IChatMessagesTool
{
    Task<IEnumerable<string>> ListRecentMessagesAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken);
    Task<IEnumerable<string>> GetChatMessagesAsync(Guid sessionId, CancellationToken cancellationToken);
}
