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

    public Task<IntentMatch?> ClassifyAsync(
        string message,
        IReadOnlyList<string>? priorUserMessages = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return Task.FromResult<IntentMatch?>(null);
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
                    return Task.FromResult<IntentMatch?>(new IntentMatch(intent, new Dictionary<string, string> { [capture.CaptureName] = value }));
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
                    return Task.FromResult<IntentMatch?>(new IntentMatch(intent));
                }
            }
        }

        var priorMessage = priorUserMessages is { Count: > 0 } ? priorUserMessages[^1] : null;
        if (!string.IsNullOrWhiteSpace(priorMessage))
        {
            foreach (var intent in _catalog.Intents)
            {
                if (intent.FollowUpExamples?.Any(example => priorMessage.Trim().Equals(example, StringComparison.OrdinalIgnoreCase)) == true)
                {
                    return Task.FromResult<IntentMatch?>(new IntentMatch(intent, new Dictionary<string, string>
                    {
                        ["followUp"] = message.Trim()
                    }));
                }
            }
        }

        return Task.FromResult<IntentMatch?>(null);
    }
}
