namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// Deterministic rule-based <see cref="IIntentClassifier"/>: matches a message against each
/// <see cref="IntentDefinition"/>'s <see cref="IntentDefinition.Captures"/> (readable phrase-capture
/// matching, checked first so parameterized intents win over broad phrase matches) and then
/// <see cref="IntentDefinition.Examples"/> (case-insensitive substring). No external calls, no model
/// inference - this is pure, fast, and fully unit-testable.
/// </summary>
public sealed class RuleIntentClassifier(IntentCatalog catalog) : IIntentClassifier
{
    private readonly IntentCatalog _catalog = catalog;

    public IntentMatch? Classify(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        foreach (var intent in _catalog.Intents)
        {
            if (intent.Captures is null)
            {
                continue;
            }

            foreach (var capture in intent.Captures)
            {
                if (capture.TryMatch(message, out var value))
                {
                    return new IntentMatch(intent, new Dictionary<string, string> { [capture.CaptureName] = value });
                }
            }
        }

        var normalized = message.ToLowerInvariant();
        foreach (var intent in _catalog.Intents)
        {
            foreach (var example in intent.Examples)
            {
                if (normalized.Contains(example, StringComparison.Ordinal))
                {
                    return new IntentMatch(intent);
                }
            }
        }

        return null;
    }
}
