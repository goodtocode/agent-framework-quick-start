namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// The ordered collection of all known deterministic chat intents. Replaces inline conditional logic
/// with declarative data that can be unit tested independently of routing/dispatch logic.
/// </summary>
public sealed class IntentCatalog(IEnumerable<IntentDefinition> intents)
{
    /// <summary>
    /// All registered intents, in priority order. <see cref="IIntentClassifier"/> implementations
    /// should evaluate them in this order so earlier, more specific intents (e.g. parameterized
    /// selection intents) win over later, broader ones when a message could match more than one.
    /// </summary>
    public IReadOnlyList<IntentDefinition> Intents { get; } = [.. intents];
}
