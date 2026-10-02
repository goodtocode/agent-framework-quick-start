using Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;
using Goodtocode.Agents.Playbook.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks.SqlStatistics;

/// <summary>
/// Discovers and registers the SQL Statistics Classification example playbook's
/// Collect/Evaluate/Record tools, and registers the keyed resolver used to select a stage's tool
/// per execution.
/// </summary>
public static class SqlStatisticsPlaybookToolRegistration
{
    public static readonly PlaybookToolKey DefaultCollectKey = PlaybookToolKey.Create("sql-statistics.deterministic.collect");

    public static readonly PlaybookToolKey DefaultEvaluateKey = PlaybookToolKey.Create("sql-statistics.deterministic.evaluate");

    public static readonly PlaybookToolKey DefaultRecordKey = PlaybookToolKey.Create("sql-statistics.deterministic.record");

    public static IServiceCollection AddSqlStatisticsPlaybookTools(this IServiceCollection services)
    {
        services.AddPlaybookStepTools<string, SqlDatabaseStatisticsEvidence, SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>(
            typeof(SqlStatisticsPlaybookToolRegistration).Assembly);

        services.AddScoped<IPlaybookStepToolResolver<string, SqlDatabaseStatisticsEvidence, SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>>(
            provider => new KeyedPlaybookStepToolResolver<string, SqlDatabaseStatisticsEvidence, SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>(
                provider,
                DefaultCollectKey,
                DefaultEvaluateKey,
                DefaultRecordKey));

        return services;
    }
}
