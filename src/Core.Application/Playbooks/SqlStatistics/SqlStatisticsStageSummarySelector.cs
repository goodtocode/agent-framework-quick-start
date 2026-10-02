using Goodtocode.AgentFramework.Core.Application.Playbooks.Governance;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;

/// <summary>
/// Projects the SQL Statistics Classification playbook's CER outputs into governance summaries,
/// consumed by <see cref="SharedPlaybookGovernanceActivityRecorder{TEvidence,TFinding,TMaterialization}"/>.
/// </summary>
public sealed class SqlStatisticsStageSummarySelector
    : IPlaybookStageSummarySelector<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>
{
    public string SummarizeEvidence(SqlDatabaseStatisticsEvidence evidence) =>
        $"Database: {evidence.DatabaseName}, SizeGb: {evidence.SizeGb}, TableCount: {evidence.TableCount}, IndexCount: {evidence.IndexCount}";

    public string SummarizeFinding(SqlDatabaseSizeFinding finding) =>
        $"Database: {finding.DatabaseName}, Classification: {finding.Classification}, Reason: {finding.Reason}";

    public string SummarizeMaterialization(SqlDatabaseSizeMaterialization materialization) => materialization.Summary;
}
