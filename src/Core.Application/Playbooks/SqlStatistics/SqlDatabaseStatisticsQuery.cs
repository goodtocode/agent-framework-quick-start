namespace Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;

/// <summary>
/// Framework-agnostic abstraction over the deterministic retrieval of SQL Server database
/// statistics, implemented by <c>Infrastructure.SqlServer</c>. Keeps <see cref="SqlDatabaseStatisticsQueryHandler"/>
/// free of EF/SQL-specific details per the Core.Application layer boundary.
/// </summary>
public interface ISqlDatabaseStatisticsProvider
{
    Task<SqlDatabaseStatisticsEvidence> GetStatisticsAsync(string databaseName, CancellationToken cancellationToken);
}

/// <summary>
/// Collect-stage input/application query for the SQL Statistics Classification example playbook.
/// This is a deterministic application query: the Collect stage dispatches it directly, it is
/// never exposed as an LLM-selectable tool.
/// </summary>
public sealed class SqlDatabaseStatisticsQuery : IRequest<SqlDatabaseStatisticsEvidence>
{
    public required string DatabaseName { get; init; }
}

public sealed class SqlDatabaseStatisticsQueryHandler(ISqlDatabaseStatisticsProvider provider)
    : IRequestHandler<SqlDatabaseStatisticsQuery, SqlDatabaseStatisticsEvidence>
{
    private readonly ISqlDatabaseStatisticsProvider _provider = provider;

    public Task<SqlDatabaseStatisticsEvidence> Handle(SqlDatabaseStatisticsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _provider.GetStatisticsAsync(request.DatabaseName, cancellationToken);
    }
}
