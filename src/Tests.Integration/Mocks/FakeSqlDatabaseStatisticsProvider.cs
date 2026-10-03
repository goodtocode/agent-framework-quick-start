using Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;

namespace Goodtocode.AgentFramework.Tests.Integration.Mocks;

/// <summary>
/// Deterministic test double for <see cref="ISqlDatabaseStatisticsProvider"/>, configurable per
/// test. The real <c>SqlServerDatabaseStatisticsProvider</c> relies on raw SQL against catalog
/// views that the InMemory EF provider used by <see cref="TestBase"/> cannot execute.
/// </summary>
public sealed class FakeSqlDatabaseStatisticsProvider : ISqlDatabaseStatisticsProvider
{
    public double SizeGb { get; set; } = 1;

    public int TableCount { get; set; } = 10;

    public int IndexCount { get; set; } = 20;

    public Task<SqlDatabaseStatisticsEvidence> GetStatisticsAsync(string databaseName, CancellationToken cancellationToken) =>
        Task.FromResult(new SqlDatabaseStatisticsEvidence(databaseName, SizeGb, TableCount, IndexCount, DateTimeOffset.UtcNow));
}
