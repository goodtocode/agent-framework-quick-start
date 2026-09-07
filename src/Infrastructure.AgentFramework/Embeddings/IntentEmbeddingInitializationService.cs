using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Embeddings;

/// <summary>
/// Seeds intent example embeddings once at startup when semantic classification is enabled.
/// </summary>
public sealed class IntentEmbeddingInitializationService(
    IServiceScopeFactory scopeFactory,
    IOptions<IntentClassificationOptions> options,
    ILogger<IntentEmbeddingInitializationService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.EnableSemantic)
        {
            logger.LogInformation("Semantic intent classification is disabled; skipping embedding initialization.");
            return;
        }

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var store = scope.ServiceProvider.GetRequiredService<IIntentEmbeddingStore>();
            if (await store.IsReadyAsync(cancellationToken))
            {
                logger.LogInformation("Intent embedding store is already initialized; skipping seeding.");
                return;
            }

            var catalog = scope.ServiceProvider.GetRequiredService<IntentCatalog>();
            var generator = scope.ServiceProvider.GetRequiredService<IEmbeddingGenerator>();

            foreach (var intent in catalog.Intents)
            {
            var examples = intent.Examples
                .Where(example => !string.IsNullOrWhiteSpace(example))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
                if (examples.Length == 0)
                {
                    continue;
                }

                var vectors = await generator.GenerateBatchAsync(examples, cancellationToken);
            var embeddings = examples
                .Where(vectors.ContainsKey)
                .Select(example => new Embedding
                {
                    Id = Guid.NewGuid(),
                    IntentName = intent.Name,
                    Source = EmbeddingSource.Example,
                    SourceText = example,
                    Vector = vectors[example],
                    Weight = 1f,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                });

                await store.UpsertIntentEmbeddingsAsync(intent.Name, embeddings, cancellationToken);
            }

            logger.LogInformation("Intent embedding initialization completed for {IntentCount} intents.", catalog.Intents.Count);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Intent embedding initialization failed; continuing with rule-based classification.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
