using Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Journeys;

/// <summary>
/// Declarative definition of the suggested prompts offered at one <see cref="ChatJourneyLevel"/>.
/// A definition with a <see cref="ScopeCode"/> wins over the global (null-scope) definition for the
/// same level, allowing a specialized chat experience to override the default journey.
/// </summary>
public sealed record ChatJourneyDefinition(
    ChatJourneyLevel Level,
    IReadOnlyList<string> SuggestedPrompts,
    string? ScopeCode = null);
