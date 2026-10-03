using Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;
using Goodtocode.Agents.Playbook.Execution;
using Goodtocode.Agents.Playbook.Steps;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks.SqlStatistics;

/// <summary>
/// Deterministic Evaluate stage for the SQL Statistics Classification example playbook:
/// classifies the collected database size against the versioned <see cref="SqlStatisticsKnowledgeHolder"/>
/// rubric's <see cref="ContinuousEvaluationScale"/> bands. No LLM involvement.
/// </summary>
[PlaybookTool("sql-statistics.deterministic.evaluate")]
public sealed class SqlStatisticsEvaluateTool(SqlStatisticsKnowledgeHolder knowledgeHolder)
    : IEvaluateStepTool<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding>
{
    private readonly PlaybookKnowledge _knowledge = knowledgeHolder.Knowledge;

    public string ToolName => "sql-statistics.deterministic.evaluate";

    public Task<SqlDatabaseSizeFinding> EvaluateAsync(SqlDatabaseStatisticsEvidence evidence, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        var scale = (ContinuousEvaluationScale)_knowledge.Rubric.Scale;
        var orderedEntries = scale.Entries.OrderBy(entry => entry.Maximum).ToList();
        var matchedEntry = orderedEntries.FirstOrDefault(entry => evidence.SizeGb <= entry.Maximum) ?? orderedEntries[^1];

        var classification = Enum.Parse<SqlDatabaseSizeClassification>(matchedEntry.Name);
        var reason = $"Database size {evidence.SizeGb:0.##} GB falls in the '{matchedEntry.Name}' band ({matchedEntry.Minimum:0.##}-{matchedEntry.Maximum:0.##} GB) of rubric version {_knowledge.Rubric.Version}.";

        return Task.FromResult(new SqlDatabaseSizeFinding(evidence.DatabaseName, evidence.SizeGb, classification, reason, _knowledge.Rubric.Version));
    }
}
