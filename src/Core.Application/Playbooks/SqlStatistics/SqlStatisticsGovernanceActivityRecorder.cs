using Goodtocode.AgentFramework.Core.Application.Common.Auth;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Governance;
using Goodtocode.Agents.Governance.Application;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;

/// <summary>
/// SQL Statistics Classification example playbook's governance activity recorder: a thin
/// specialization of <see cref="SharedPlaybookGovernanceActivityRecorder{TEvidence,TFinding,TMaterialization}"/>
/// supplying <see cref="SqlStatisticsStageSummarySelector"/> for stage summaries.
/// </summary>
public sealed class SqlStatisticsGovernanceActivityRecorder(IRlsContext userContext, IRepeatabilityHashStrategy hashStrategy)
    : SharedPlaybookGovernanceActivityRecorder<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>(
        userContext,
        hashStrategy,
        new SqlStatisticsStageSummarySelector());
