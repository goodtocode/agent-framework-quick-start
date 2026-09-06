namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence.Entities;

/// <summary>
/// EF Core entity representing a single intent embedding vector stored in SQL Server.
/// Maps to the [Chat].[IntentEmbeddings] table.
/// Immutable after creation (used for storage/retrieval only; mutations via SemanticIntentClassifier abstraction).
/// </summary>
public class IntentEmbeddingEntity
{
    /// <summary>
    /// Creates an embedding persistence entity.
    /// </summary>
    public IntentEmbeddingEntity()
    {
    }

    /// <summary>
    /// Unique identifier (primary key).
    /// Generated as Guid.NewGuid() when creating an embedding for storage.
    /// </summary>
    public virtual Guid Id { get; set; }

    /// <summary>
    /// Associated intent name (e.g., "CreatePlaybook").
    /// Used for grouping and filtering during search.
    /// Foreign key concept, though no explicit constraint (loose coupling to IntentDefinition).
    /// </summary>
    public virtual string IntentName { get; set; } = null!;

    /// <summary>
    /// Categorization of embedding source (Example=0, Description=1, ToolMetadata=2).
    /// Stored as integer for efficiency; mapped from EmbeddingSource enum.
    /// </summary>
    public virtual int Source { get; set; }

    /// <summary>
    /// Original text from which the embedding was generated.
    /// Preserved for reference, logging, and user-facing display of match context.
    /// </summary>
    public virtual string SourceText { get; set; } = null!;

    /// <summary>
    /// Embedding vector (1536-dim for text-embedding-3-small).
    /// Stored as float array, serialized/deserialized by EF Core value converter.
    /// </summary>
    public virtual float[] Vector { get; set; } = null!;

    /// <summary>
    /// Weight applied during similarity scoring (default 1.0).
    /// Enables prioritizing certain embedding sources over others.
    /// Stored as float in database for future SIMD scoring.
    /// </summary>
    public virtual float Weight { get; set; } = 1.0f;

    /// <summary>
    /// Timestamp when embedding was created (UTC).
    /// Set by EF Core on insert; never updated.
    /// </summary>
    public virtual DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Timestamp when embedding was last updated (UTC).
    /// Updated by EF Core on upsert operations.
    /// </summary>
    public virtual DateTime UpdatedAtUtc { get; set; }
}
