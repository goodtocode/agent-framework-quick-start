namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// Classifies a raw chat message into a known <see cref="IntentDefinition"/>, or returns
/// <see langword="null"/> when nothing matches confidently (the caller should then fall back to the
/// AI agent's own tool-calling). This is the only public factory of <see cref="IntentMatch"/> -
/// routing code cannot obtain a match by any other means, which structurally enforces
/// classify-before-route.
/// </summary>
public interface IIntentClassifier
{
    /// <summary>Attempts to classify <paramref name="message"/> against the registered <see cref="IntentCatalog"/>.</summary>
    IntentMatch? Classify(string message);
}
