namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Embeddings;

/// <summary>
/// Immutable value type representing a single search result from semantic similarity matching.
/// Returned by IIntentEmbeddingStore.SearchAsync to communicate which intent matched
/// and with what confidence level.
/// </summary>
public sealed record EmbeddingMatch
{
    /// <summary>
    /// Intent name that matched the search query.
    /// Used by the classifier to identify which intent definition to return.
    /// </summary>
    public string IntentName { get; init; } = null!;

    /// <summary>
    /// Source of the matched embedding (Example, Description, ToolMetadata).
    /// Provides context about the reliability/weight of the match.
    /// </summary>
    public EmbeddingSource Source { get; init; }

    /// <summary>
    /// The original text associated with the matched embedding.
    /// Useful for logging, debugging, and explaining to users why a match occurred.
    /// Example: "create a new playbook" (the exact example that matched).
    /// </summary>
    public string SourceText { get; init; } = null!;

    /// <summary>
    /// Cosine similarity score between the query vector and this embedding's vector.
    /// Range: 0.0 (completely dissimilar) to 1.0 (identical).
    /// Typically compared against a threshold (e.g., 0.75) to determine match acceptance.
    /// </summary>
    public float SimilarityScore { get; init; }
}
