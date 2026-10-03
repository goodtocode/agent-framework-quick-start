namespace Goodtocode.AgentFramework.Core.Domain.Playbooks;

/// <summary>
/// The three, and only three, stages a Playbook step can be. Every <see cref="PlaybookEntity"/>
/// has exactly one <see cref="PlaybookStepEntity"/> per value of this enum.
/// </summary>
public enum PlaybookStepType
{
    Collect = 0,
    Evaluate = 1,
    Record = 2
}
