using Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;
using Goodtocode.Agents.Playbook.Steps;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks.SqlStatistics;

/// <summary>
/// Deterministic Record stage for the SQL Statistics Classification example playbook: shapes the
/// finding into a display/persistence-ready materialization. No LLM involvement and no additional
/// collection or evaluation.
/// </summary>
[PlaybookTool("sql-statistics.deterministic.record")]
public sealed class SqlStatisticsRecordTool : IRecordStepTool<SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>
{
    public string ToolName => "sql-statistics.deterministic.record";

    public Task<SqlDatabaseSizeMaterialization> RecordAsync(SqlDatabaseSizeFinding finding, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(finding);

        var summary = $"Database classified as {finding.Classification} according to rubric {finding.RubricVersion}.";
        return Task.FromResult(new SqlDatabaseSizeMaterialization(finding.DatabaseName, finding.Classification, finding.SizeGb, summary));
    }
}
