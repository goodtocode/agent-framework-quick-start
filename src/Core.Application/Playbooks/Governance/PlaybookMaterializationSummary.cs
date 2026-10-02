using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Governance;

/// <summary>
/// Marks a Record-stage materialization as carrying a human-readable <see cref="Summary"/> string,
/// so every example playbook's Record output is reachable through the same simple string-shaped
/// accessor (<see cref="PlaybookExecutionResultExtensions.Summary{TEvidence,TFinding,TMaterialization}"/>),
/// regardless of how much richer, typed detail the materialization also carries.
/// </summary>
public interface IPlaybookMaterializationSummary
{
    string Summary { get; }
}

/// <summary>
/// Shared convenience accessor so callers who only want the Record stage's plain-text summary -
/// the easiest-to-understand shape of a playbook's output - never need to learn each playbook's
/// specific materialization type.
/// </summary>
public static class PlaybookExecutionResultExtensions
{
    public static string Summary<TEvidence, TFinding, TMaterialization>(
        this PlaybookExecutionResult<TEvidence, TFinding, TMaterialization> result)
        where TMaterialization : IPlaybookMaterializationSummary
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Materialization.Summary;
    }
}
