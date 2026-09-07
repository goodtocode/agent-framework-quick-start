namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Embeddings;

/// <summary>
/// Categorizes the origin/type of an embedding vector.
/// Used to distinguish between user-authored examples, system descriptions, and derived metadata,
/// allowing weighted scoring and filtering during semantic search.
/// </summary>
public enum EmbeddingSource
{
    /// <summary>
    /// Embedding generated from a user-written example phrasing (e.g., from IntentDefinition.Examples).
    /// Weight: 1.0 (highest priority; users wrote these).
    /// </summary>
    Example = 0,

    /// <summary>
    /// Embedding generated from a tool or intent description (e.g., from IntentDefinition.Description).
    /// Weight: 0.8 (lower priority; tool authors wrote these; less user-centric).
    /// </summary>
    Description = 1,

    /// <summary>
    /// Embedding generated from tool metadata, field names, or derived context.
    /// Weight: 0.6 (lowest priority; system-generated; less reliable).
    /// </summary>
    ToolMetadata = 2
}
