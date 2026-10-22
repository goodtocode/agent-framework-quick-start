using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;

/// <summary>
/// Options for the governed Agentic Intent chat client.
/// </summary>
public sealed class GovernedIntentOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "GovernedIntent";

    /// <summary>
    /// Gets or sets whether a turn fails closed when no policy resolves.
    /// </summary>
    public bool IntentRequired { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the per-message Preamble lists principle ids (compact) or full text.
    /// </summary>
    public GovernancePreambleMode PreambleMode { get; set; } = GovernancePreambleMode.Compact;
}
