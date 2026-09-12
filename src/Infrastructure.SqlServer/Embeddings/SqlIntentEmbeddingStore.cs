using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Embeddings;
using Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence;
using Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence.Entities;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Embeddings;

/// <summary>
/// SQL Server implementation of IIntentEmbeddingStore.
/// Stores embeddings in [Chat].[IntentEmbeddings] table and performs cosine similarity search.
/// For ~2500 embeddings, brute-force similarity computation is acceptable (<100ms).
/// </summary>
public sealed class SqlIntentEmbeddingStore : IIntentEmbeddingStore
{
    private readonly AgentFrameworkContext _context;
    private readonly ILogger<SqlIntentEmbeddingStore> _logger;

    /// <summary>
    /// Creates a new instance of the SQL Server embedding store.
    /// </summary>
    /// <param name="context">EF Core database context.</param>
    /// <param name="logger">Structured logger.</param>
    public SqlIntentEmbeddingStore(
        AgentFrameworkContext context,
        ILogger<SqlIntentEmbeddingStore> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task UpsertIntentEmbeddingsAsync(
        string intentName,
        IEnumerable<Embedding> embeddings,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(intentName))
            throw new ArgumentException("Intent name cannot be null or whitespace", nameof(intentName));

        var embeddingsList = embeddings.ToList();
        if (embeddingsList.Count == 0)
        {
            _logger.LogWarning("No embeddings provided for intent {IntentName}", intentName);
            return;
        }

        try
        {
            // Delete existing embeddings for this intent
            var existing = await _context.IntentEmbeddings
                .Where(e => e.IntentName == intentName)
                .ToListAsync(cancellationToken);

            if (existing.Count > 0)
            {
                _context.IntentEmbeddings.RemoveRange(existing);
                _logger.LogInformation(
                    "Deleted {Count} existing embeddings for intent {IntentName}",
                    existing.Count,
                    intentName);
            }

            // Insert new embeddings
            var entities = embeddingsList.Select(e => new IntentEmbeddingEntity
            {
                Id = Guid.NewGuid(),
                IntentName = intentName,
                Source = (int)e.Source,
                SourceText = e.SourceText,
                Vector = e.Vector,
                Weight = e.Weight,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            }).ToList();

            await _context.IntentEmbeddings.AddRangeAsync(entities, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Upserted {Count} embeddings for intent {IntentName}",
                entities.Count,
                intentName);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Upsert operation was cancelled for intent {IntentName}", intentName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting embeddings for intent {IntentName}", intentName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EmbeddingMatch>> SearchAsync(
        float[] queryVector,
        CancellationToken cancellationToken,
        int topK = 5,
        float similarityThreshold = 0.75f,
        IReadOnlySet<string>? eligibleIntentNames = null)
    {
        if (queryVector == null || queryVector.Length == 0)
            throw new ArgumentException("Query vector cannot be null or empty", nameof(queryVector));

        if (topK <= 0)
            throw new ArgumentException("topK must be > 0", nameof(topK));

        if (similarityThreshold < 0 || similarityThreshold > 1)
            throw new ArgumentException("similarityThreshold must be between 0 and 1", nameof(similarityThreshold));

        try
        {
            if (eligibleIntentNames is { Count: 0 })
            {
                return Array.Empty<EmbeddingMatch>();
            }

            var eligibleNames = eligibleIntentNames?.ToArray();
            // Load all embeddings (brute-force acceptable for ~2500 embeddings)
            var embeddingsQuery = _context.IntentEmbeddings.AsNoTracking();
            if (eligibleNames is not null)
            {
                embeddingsQuery = embeddingsQuery.Where(embedding => eligibleNames.Contains(embedding.IntentName));
            }

            var allEmbeddings = await embeddingsQuery.ToListAsync(cancellationToken);

            if (allEmbeddings.Count == 0)
            {
                _logger.LogDebug("No embeddings found in store");
                return Array.Empty<EmbeddingMatch>();
            }

            // Compute cosine similarity for each embedding
            var matches = new List<(EmbeddingMatch match, float score)>();

            foreach (var entity in allEmbeddings)
            {
                if (entity.Vector == null || entity.Vector.Length == 0)
                    continue;

                var similarity = CosineSimilarity(queryVector, entity.Vector);

                // Apply weight
                var weightedScore = similarity * entity.Weight;

                if (weightedScore >= similarityThreshold)
                {
                    var match = new EmbeddingMatch
                    {
                        IntentName = entity.IntentName,
                        Source = (EmbeddingSource)entity.Source,
                        SourceText = entity.SourceText,
                        SimilarityScore = weightedScore
                    };
                    matches.Add((match, weightedScore));
                }
            }

            // Sort by similarity descending and take top K
            var results = matches
                .OrderByDescending(m => m.score)
                .Take(topK)
                .Select(m => m.match)
                .ToList();

            _logger.LogInformation(
                "Semantic search found {Count} matches (threshold={Threshold}, topK={TopK})",
                results.Count,
                similarityThreshold,
                topK);

            return results;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Search operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during semantic search");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteIntentEmbeddingsAsync(string intentName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(intentName))
            throw new ArgumentException("Intent name cannot be null or whitespace", nameof(intentName));

        try
        {
            var existing = await _context.IntentEmbeddings
                .Where(e => e.IntentName == intentName)
                .ToListAsync(cancellationToken);

            _context.IntentEmbeddings.RemoveRange(existing);
            await _context.SaveChangesAsync(cancellationToken);
            var count = existing.Count;

            _logger.LogInformation("Deleted {Count} embeddings for intent {IntentName}", count, intentName);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Delete operation was cancelled for intent {IntentName}", intentName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting embeddings for intent {IntentName}", intentName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsReadyAsync(CancellationToken cancellationToken)
    {
        try
        {
            var count = await _context.IntentEmbeddings
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var isReady = count > 0;
            _logger.LogInformation("Embedding store ready check: {Count} embeddings found", count);
            return isReady;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if embedding store is ready");
            return false;
        }
    }

    /// <summary>
    /// Computes cosine similarity between two vectors.
    /// Formula: (a · b) / (||a|| * ||b||)
    /// Result range: -1.0 to 1.0 (typically 0.0 to 1.0 for text embeddings).
    /// </summary>
    /// <param name="a">First vector (typically query).</param>
    /// <param name="b">Second vector (typically stored embedding).</param>
    /// <returns>Cosine similarity score.</returns>
    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have the same dimension", nameof(b));

        if (a.Length == 0)
            return 0f;

        // Compute dot product
        double dotProduct = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
        }

        // Compute magnitudes
        double magnitudeA = 0;
        double magnitudeB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        magnitudeA = Math.Sqrt(magnitudeA);
        magnitudeB = Math.Sqrt(magnitudeB);

        // Avoid division by zero
        if (magnitudeA == 0 || magnitudeB == 0)
            return 0f;

        // Return normalized dot product
        return (float)(dotProduct / (magnitudeA * magnitudeB));
    }
}
