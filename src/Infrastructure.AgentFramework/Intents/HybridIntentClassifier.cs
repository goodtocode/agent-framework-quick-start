using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// Runs deterministic classification first and semantic classification only when rules do not match.
/// </summary>
public sealed class HybridIntentClassifier(
    RuleIntentClassifier ruleClassifier,
    SemanticIntentClassifier semanticClassifier,
    IOptions<IntentClassificationOptions> options,
    ILogger<HybridIntentClassifier> logger) : IIntentClassifier
{
    public async Task<IntentMatch?> ClassifyAsync(
        string message,
        IReadOnlyList<string>? priorUserMessages = null,
        CancellationToken cancellationToken = default)
    {
        var ruleMatch = await ruleClassifier.ClassifyAsync(message, priorUserMessages, cancellationToken);
        if (ruleMatch is not null || !options.Value.EnableSemantic)
        {
            return ruleMatch;
        }

        logger.LogDebug("No rule intent match; attempting semantic classification.");
        try
        {
            return await semanticClassifier.ClassifyAsync(message, priorUserMessages, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Semantic intent classification failed; falling back to the agent.");
            return null;
        }
    }
}
