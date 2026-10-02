using Goodtocode.Agents.Playbook.Steps;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;

/// <summary>
/// SQL Statistics Classification example playbook (Workflow Type 1): a fully deterministic,
/// framework-agnostic CER workflow. Resolves its Collect/Evaluate/Record tools per execution via
/// the shared resolver so this definition never changes even though no stage is agentic. The
/// Collect-stage input is the plain database name string, matching the string-shaped Collect
/// input convention shared by every example playbook; <see cref="SqlStatisticsCollectTool"/>
/// alone is responsible for turning that string into the typed <see cref="SqlDatabaseStatisticsQuery"/>
/// dispatched through the application mediator pipeline.
/// </summary>
public sealed class SqlStatisticsPlaybookDefinition(
    IPlaybookStepToolResolver<string, SqlDatabaseStatisticsEvidence, SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization> resolver)
    : IPlaybookSteps<string, SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>
{
    public string PlaybookKey => "sql-statistics";

    public string Version => "v1";

    public ICollectStep<string, SqlDatabaseStatisticsEvidence> Collect => resolver.ResolveCollect(null);

    public IEvaluateStep<SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding> Evaluate => resolver.ResolveEvaluate(null);

    public IRecordStep<SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization> Record => resolver.ResolveRecord(null);
}
