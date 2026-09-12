using Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Journeys;

/// <summary>
/// Tier 1a suggested-prompt resolution: a deterministic catalog lookup by resolved journey level
/// and optional scope code, with no AI or embedding call. Tier 1b (embedding similarity) and
/// Tier 2 (agent-generated prompts) are deliberate follow-ups, mirroring
/// <c>SemanticIntentClassifier</c>/<c>HybridIntentClassifier</c>.
/// </summary>
public sealed class RuleChatJourneyProvider : IChatJourneyProvider
{
    private readonly ChatJourneyCatalog _catalog;

    public RuleChatJourneyProvider(ChatJourneyCatalogFactory catalogFactory)
    {
        ArgumentNullException.ThrowIfNull(catalogFactory);
        _catalog = catalogFactory.Create();
    }

    public Task<IReadOnlyList<ChatJourneySuggestedPrompt>> ResolveSuggestedPromptsAsync(
        ChatJourneyContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var definition = _catalog.Resolve(context.ResolveLevel(), context.ScopeCode);
        IReadOnlyList<ChatJourneySuggestedPrompt> prompts = definition is null
            ? []
            : [.. definition.SuggestedPrompts.Select(prompt => new ChatJourneySuggestedPrompt(prompt))];

        return Task.FromResult(prompts);
    }
}
