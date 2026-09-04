namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// Stable names for every deterministic chat intent. Used both by <see cref="IntentCatalog"/> entries
/// and by routing logic's switch statement, so a typo in one place fails to compile instead of silently
/// never matching.
/// </summary>
public static class IntentNames
{
    public const string QueryChatSessionsList = nameof(QueryChatSessionsList);
    public const string QueryChatMessagesList = nameof(QueryChatMessagesList);
    public const string QueryActorById = nameof(QueryActorById);
    public const string QueryActorsByName = nameof(QueryActorsByName);
    public const string QueryActorsList = nameof(QueryActorsList);
    public const string QueryMyActorsList = nameof(QueryMyActorsList);
    public const string SearchWeb = nameof(SearchWeb);
}
