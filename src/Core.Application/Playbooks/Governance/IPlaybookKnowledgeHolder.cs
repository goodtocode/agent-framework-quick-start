using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Governance;

/// <summary>
/// Thin, per-playbook wrapper around a shared <see cref="PlaybookKnowledge"/> instance. Every
/// example playbook (SQL Statistics, Taxonomy, Essay) supplies its versioned evaluation criteria
/// through this exact same mechanism - a DI singleton holder exposing the package's generic
/// <see cref="PlaybookKnowledge"/>/<see cref="EvaluationRubric"/> shape - so the only thing that
/// differs between playbooks is the criteria data itself, never the type carrying it. A distinct
/// holder record exists per playbook only so each can be registered and resolved as its own DI
/// service type (constructor injection cannot disambiguate between multiple unkeyed registrations
/// of the same <see cref="PlaybookKnowledge"/> type).
/// </summary>
public interface IPlaybookKnowledgeHolder
{
    PlaybookKnowledge Knowledge { get; }
}
