using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Embeddings;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// Classifies messages by comparing their embedding to seeded intent examples.
/// </summary>
public sealed class SemanticIntentClassifier(
    IntentCatalog catalog,
    IEmbeddingGenerator embeddingGenerator,
    IIntentEmbeddingStore embeddingStore,
    IOptions<IntentClassificationOptions> options,
    ILogger<SemanticIntentClassifier> logger) : IIntentClassifier
{
    public async Task<IntentMatch?> ClassifyAsync(
        string message,
        IReadOnlyList<string>? priorUserMessages = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var queryVector = await embeddingGenerator.GenerateAsync(message, cancellationToken);
        var matches = await embeddingStore.SearchAsync(
            queryVector,
            cancellationToken,
            options.Value.TopKResults,
            options.Value.SemanticThreshold);

        foreach (var match in matches)
        {
            var intent = catalog.Intents.FirstOrDefault(
                definition => definition.Name.Equals(match.IntentName, StringComparison.Ordinal));

            if (intent is null || intent.Captures is { Count: > 0 })
            {
                continue;
            }

            logger.LogInformation(
                "Semantic intent match resolved for {IntentName} with score {SimilarityScore}",
                intent.Name,
                match.SimilarityScore);
            return new IntentMatch(intent);
        }

        return null;
    }
}
