namespace Goodtocode.AgentFramework.Core.Domain.Playbooks;

/// <summary>
/// Describes the shape of a <see cref="PlaybookStepEntity.ActionDefinition"/>, so the
/// Presentation.Web layer can render it appropriately (syntax-highlighted query, rubric table,
/// template, or free-form prompt) without guessing from content.
/// </summary>
public enum PlaybookActionFormat
{
    SqlQuery = 0,
    Rubric = 1,
    Template = 2,
    Prompt = 3
}
