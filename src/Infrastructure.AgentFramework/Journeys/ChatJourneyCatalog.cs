using Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Journeys;

/// <summary>
/// All known <see cref="ChatJourneyDefinition"/>s. Mirrors <c>IntentCatalog</c>: declarative data
/// that can be unit tested independently of the provider that resolves against it.
/// </summary>
public sealed class ChatJourneyCatalog(IEnumerable<ChatJourneyDefinition> definitions)
{
    public IReadOnlyList<ChatJourneyDefinition> Definitions { get; } = [.. definitions];

    /// <summary>
    /// Resolves the definition for a level, preferring a scope-specific definition over the global
    /// one. Returns <c>null</c> when no definition is registered for the level.
    /// </summary>
    public ChatJourneyDefinition? Resolve(ChatJourneyLevel level, string? scopeCode)
    {
        if (!string.IsNullOrWhiteSpace(scopeCode))
        {
            var scoped = Definitions.FirstOrDefault(definition =>
                definition.Level == level
                && string.Equals(definition.ScopeCode, scopeCode, StringComparison.OrdinalIgnoreCase));

            if (scoped is not null)
            {
                return scoped;
            }
        }

        return Definitions.FirstOrDefault(definition => definition.Level == level && definition.ScopeCode is null);
    }
}
