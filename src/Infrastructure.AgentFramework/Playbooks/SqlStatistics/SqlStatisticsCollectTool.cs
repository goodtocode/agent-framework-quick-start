using Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Execution;
using Goodtocode.Agents.Playbook.Steps;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks.SqlStatistics;

/// <summary>
/// Deterministic Collect stage for the SQL Statistics Classification example playbook: takes the
/// plain database name string (the string-shaped Collect input convention shared by every example
/// playbook), builds the typed <see cref="SqlDatabaseStatisticsQuery"/>, and dispatches it through
/// the application mediator pipeline. The workflow selects this collection operation directly; it
/// is never exposed as an LLM-selectable tool.
/// </summary>
[PlaybookTool("sql-statistics.deterministic.collect")]
public sealed class SqlStatisticsCollectTool(IToolApplicationExecutor executor)
    : ICollectStepTool<string, SqlDatabaseStatisticsEvidence>
{
    private readonly IToolApplicationExecutor _executor = executor;

    public string ToolName => "sql-statistics.deterministic.collect";

    public Task<SqlDatabaseStatisticsEvidence> ExecuteAsync(string input, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        return _executor.SendAsync(new SqlDatabaseStatisticsQuery { DatabaseName = input }, cancellationToken);
    }
}
