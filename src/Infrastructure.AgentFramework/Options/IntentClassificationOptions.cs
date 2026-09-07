using System.ComponentModel.DataAnnotations;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;

/// <summary>
/// Controls semantic intent classification behavior.
/// </summary>
public sealed class IntentClassificationOptions
{
    public const string SectionName = "IntentClassification";

    /// <summary>Whether semantic fallback is enabled.</summary>
    public bool EnableSemantic { get; set; }

    /// <summary>Minimum weighted similarity required for a semantic match.</summary>
    [Range(0, 1)]
    public float SemanticThreshold { get; set; } = 0.75f;

    /// <summary>Maximum number of embedding matches requested from the store.</summary>
    [Range(1, 100)]
    public int TopKResults { get; set; } = 5;
}
