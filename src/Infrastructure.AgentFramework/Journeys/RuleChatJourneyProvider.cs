using Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Journeys;

/// <summary>
/// Deterministic suggested-prompt resolution by resolved journey level and optional scope code.
/// AI- or embedding-generated prompts are separate future extensions and do not alter this
/// catalog lookup contract.
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
