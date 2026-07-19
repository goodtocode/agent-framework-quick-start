namespace Goodtocode.AgentFramework.Presentation.Web.Components;

public sealed record VersionHistoryItem(
    int Version,
    string Status,
    Guid? EntityId = null,
    DateTimeOffset? CreatedUtc = null
);
