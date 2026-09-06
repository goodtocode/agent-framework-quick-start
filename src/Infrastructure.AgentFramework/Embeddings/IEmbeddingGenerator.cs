namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Embeddings;

/// <summary>
/// Abstracts embedding vector generation from a specific model/provider.
/// Implementations: Azure OpenAI, OpenAI public API, local models via Ollama, etc.
/// </summary>
public interface IEmbeddingGenerator
{
    /// <summary>
    /// Generates an embedding vector for a single text string.
    /// Prefer <see cref="GenerateBatchAsync"/> for multiple texts (more efficient).
    /// </summary>
    /// <param name="text">Text to embed (e.g., "create a new playbook").</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Float array of length <see cref="Dimension"/>.</returns>
    Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken);

    /// <summary>
    /// Generates embedding vectors for multiple texts in a single call.
    /// More efficient than multiple <see cref="GenerateAsync"/> calls;
    /// API providers often allow batching to reduce round-trips.
    /// </summary>
    /// <param name="texts">Enumerable of texts to embed.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Dictionary mapping input text → generated vector. Empty if inputs empty.</returns>
    Task<IDictionary<string, float[]>> GenerateBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken);

    /// <summary>
    /// Dimension of generated vectors.
    /// Standard value for text-embedding-3-small: 1536.
    /// Used for validation and storage allocation.
    /// </summary>
    int Dimension { get; }
}
