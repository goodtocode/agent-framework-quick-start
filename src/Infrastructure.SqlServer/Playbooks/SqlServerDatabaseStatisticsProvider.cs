using Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;
using Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence;
using Microsoft.Data.SqlClient;

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
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        const string sql = """
            DECLARE @db sysname = @p0;

            IF DB_ID(@db) IS NULL
                THROW 50001, 'The specified database does not exist on the configured SQL Server instance.', 1;

            IF HAS_DBACCESS(@db) <> 1
                THROW 50002, 'The configured SQL credential does not have access to the specified database.', 1;

            DECLARE @quotedDb nvarchar(258) = QUOTENAME(@db);
            DECLARE @statement nvarchar(max) = N'
                SELECT
                    CAST((SELECT ISNULL(SUM(size), 0) FROM ' + @quotedDb + '.sys.database_files WHERE type IN (0, 1)) * 8.0 / 1024 / 1024 AS FLOAT) AS SizeGb,
                    (SELECT COUNT(*) FROM ' + @quotedDb + '.sys.tables) AS TableCount,
                    (SELECT COUNT(*) FROM ' + @quotedDb + '.sys.indexes WHERE index_id > 0) AS IndexCount';

            EXEC sp_executesql @statement;
            """;

        await using var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var databaseParameter = new SqlParameter("@p0", databaseName);
        command.Parameters.Add(databaseParameter);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("The SQL statistics query returned no result.");

        var row = new SqlDatabaseStatisticsRow
        {
            SizeGb = reader.GetDouble(reader.GetOrdinal("SizeGb")),
            TableCount = reader.GetInt32(reader.GetOrdinal("TableCount")),
            IndexCount = reader.GetInt32(reader.GetOrdinal("IndexCount"))
        };

        return new SqlDatabaseStatisticsEvidence(
            DatabaseName: databaseName,
            SizeGb: row.SizeGb,
            TableCount: row.TableCount,
            IndexCount: row.IndexCount,
            CollectedUtc: DateTimeOffset.UtcNow);
    }

    public class SqlDatabaseStatisticsRow
    {
        public virtual double SizeGb { get; set; }
        public virtual int TableCount { get; set; }
        public virtual int IndexCount { get; set; }
    }
}
