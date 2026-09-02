namespace Goodtocode.AgentFramework.Core.Application.Abstractions;

public interface IMyChatSessionsTool
{
    Task<IEnumerable<string>> ListRecentSessionsAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken);
    Task<string?> UpdateChatSessionTitleAsync(Guid sessionId, string newTitle, CancellationToken cancellationToken);
}
