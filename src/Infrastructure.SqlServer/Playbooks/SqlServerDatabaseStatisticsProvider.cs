using Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;
using Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Playbooks;

/// <summary>
/// SQL Server-specific implementation of <see cref="ISqlDatabaseStatisticsProvider"/>: queries the
/// connected database's catalog views deterministically for size and object counts. No LLM
/// involvement; this is the Collect-stage data source for the SQL Statistics Classification
/// example playbook.
/// </summary>
public sealed class SqlServerDatabaseStatisticsProvider(AgentFrameworkContext context) : ISqlDatabaseStatisticsProvider
{
    private readonly AgentFrameworkContext _context = context;

    public async Task<SqlDatabaseStatisticsEvidence> GetStatisticsAsync(string databaseName, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                CAST((SELECT ISNULL(SUM(size), 0) FROM sys.database_files WHERE type IN (0, 1)) * 8.0 / 1024 / 1024 AS FLOAT) AS SizeGb,
                (SELECT COUNT(*) FROM sys.tables) AS TableCount,
                (SELECT COUNT(*) FROM sys.indexes WHERE index_id > 0) AS IndexCount
            """;

        var row = await _context.Database
            .SqlQueryRaw<SqlDatabaseStatisticsRow>(sql)
            .SingleAsync(cancellationToken);

        return new SqlDatabaseStatisticsEvidence(
            DatabaseName: databaseName,
            SizeGb: row.SizeGb,
            TableCount: row.TableCount,
            IndexCount: row.IndexCount,
            CollectedUtc: DateTimeOffset.UtcNow);
    }

    private sealed class SqlDatabaseStatisticsRow
    {
        public double SizeGb { get; init; }
        public int TableCount { get; init; }
        public int IndexCount { get; init; }
    }
}
