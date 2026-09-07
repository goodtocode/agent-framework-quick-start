namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Embeddings;

/// <summary>
/// Immutable value type representing a single embedding vector and its metadata.
/// Used as the data transfer object between embedding generation, storage, and retrieval.
/// Not persisted directly; EF Core maps this to IntentEmbeddingEntity.
/// </summary>
public sealed record Embedding
{
    /// <summary>
    /// Unique identifier for this embedding (primary key in storage).
    /// Generated as a new Guid when creating an embedding for storage.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Associated intent name (e.g., "CreatePlaybook", "QueryPipelines").
    /// Used to group embeddings by intent and for filtering during search.
    /// </summary>
    public string IntentName { get; init; } = null!;

    /// <summary>
    /// Categorization of the embedding source: Example, Description, or ToolMetadata.
    /// Enables filtering and weighted scoring during similarity search.
    /// </summary>
    public EmbeddingSource Source { get; init; }

    /// <summary>
    /// Original text from which the embedding was generated.
    /// Preserved for reference, logging, and user-facing display of match context.
    /// Example: "create a new playbook" or "Creates a playbook for evaluation".
    /// </summary>
    public string SourceText { get; init; } = null!;

    /// <summary>
    /// Vector array (typically 1536-dim for text-embedding-3-small).
    /// Serialized/deserialized by the backend store (SQL VECTOR, Azure Blob, etc.).
    /// </summary>
    public float[] Vector { get; init; } = null!;

    /// <summary>
    /// Weight applied during similarity scoring (default 1.0).
    /// Examples: 1.0 (highest), Descriptions: 0.8, ToolMetadata: 0.6.
    /// Allows prioritizing certain embedding sources over others.
    /// </summary>
    public float Weight { get; init; } = 1.0f;

    /// <summary>
    /// Timestamp when the embedding was created (UTC).
    /// Used for auditing and determining embedding freshness.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the embedding was last updated (UTC).
    /// Used for auditing and cache invalidation.
    /// </summary>
    public DateTime UpdatedAtUtc { get; init; } = DateTime.UtcNow;
}
