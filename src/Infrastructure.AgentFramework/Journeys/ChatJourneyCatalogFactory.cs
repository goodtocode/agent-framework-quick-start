namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Journeys;

/// <summary>
/// Aggregates every registered <see cref="IChatJourneyCatalogContributor"/> into a single
/// <see cref="ChatJourneyCatalog"/>. Mirrors <c>DefaultIntentCatalogFactory</c>.
/// </summary>
public sealed class ChatJourneyCatalogFactory(IEnumerable<IChatJourneyCatalogContributor> contributors)
{
    private readonly IReadOnlyList<IChatJourneyCatalogContributor> _contributors = [.. contributors];

    public ChatJourneyCatalog Create()
        => new(_contributors.SelectMany(contributor => contributor.GetDefinitions()));
}
