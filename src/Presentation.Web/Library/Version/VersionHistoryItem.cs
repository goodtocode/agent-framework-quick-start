namespace Goodtocode.AgentFramework.Presentation.Web.Library.Version;

public sealed record VersionHistoryItem(
    int Version,
    string Status,
    Guid? EntityId = null,
    DateTimeOffset? CreatedUtc = null
);
