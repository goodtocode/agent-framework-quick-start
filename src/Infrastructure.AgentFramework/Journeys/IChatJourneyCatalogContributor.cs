namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Journeys;

/// <summary>
/// Extension point allowing downstream projects to contribute additional
/// <see cref="ChatJourneyDefinition"/>s at DI composition time without modifying this library.
/// </summary>
public interface IChatJourneyCatalogContributor
{
    IReadOnlyList<ChatJourneyDefinition> GetDefinitions();
}
