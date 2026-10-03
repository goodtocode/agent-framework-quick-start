namespace Goodtocode.AgentFramework.Presentation.Web.Features.Playbooks.Models;

/// <summary>
/// The three Playbook CER stages a user can toggle between in the result panel: Collect,
/// Evaluate, Record. Every example playbook page uses this same enum to drive the toggle-button
/// group and the step detail panel, regardless of the playbook's own typed evidence/finding/
/// materialization shapes.
/// </summary>
public enum PlaybookCerStep
{
    Collect,
    Evaluate,
    Record
}
