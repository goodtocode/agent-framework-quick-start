using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Application.Playbooks;
using Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;
using Goodtocode.AgentFramework.Core.Domain.Playbooks;
using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.AgentFramework.Tests.Integration.Playbooks;

[Binding]
[Scope(Tag = "playbookRepeatability")]
public sealed class PlaybookRepeatabilityStepDefinitions : TestBase
{
    private PlaybookExecutionResultDto? _original;
    private PlaybookExecutionResultDto? _repeat;
    private Exception? _rejection;

    [Given("a SQL statistics playbook is registered")]
    public async Task GivenASqlStatisticsPlaybookIsRegistered()
    {
        await Sender.Send(new CreatePlaybookCommand
        {
            Key = "sql-statistics",
            Name = "SQL Statistics Classification",
            Description = "Deterministic CER workflow for repeatability tests.",
            WorkflowType = "Deterministic",
            Version = "v1",
            Collect = Step("Collect database statistics", PlaybookActionFormat.SqlQuery, "SELECT 1;"),
            Evaluate = Step("Classify database size", PlaybookActionFormat.Rubric, "Small: 0-20 GB; Medium: 20-100 GB; Large: 100+ GB."),
            Record = Step("Record classification", PlaybookActionFormat.Template, "{DatabaseName} is {Classification} at {SizeGb} GB.")
        }, CancellationToken.None);
    }

    [Given("the connected database reports a size of (.*) GB")]
    [Given("the connected database then reports a size of (.*) GB")]
    public void GivenTheConnectedDatabaseReportsASizeOfGb(double sizeGb)
    {
        sqlDatabaseStatisticsProvider.SizeGb = sizeGb;
    }

    [Given("I have rerun the SQL statistics playbook for database \"(.*)\"")]
    public async Task GivenIHaveRerunTheSqlStatisticsPlaybookForDatabase(string database)
    {
        _original = await Sender.Send(new RunSqlStatisticsPlaybookCommand { DatabaseName = database }, CancellationToken.None);
    }

    [When("I repeat the SQL statistics playbook in \"(.*)\" mode")]
    public async Task WhenIRepeatTheSqlStatisticsPlaybookInMode(string mode)
    {
        var command = new RunSqlStatisticsPlaybookCommand
        {
            DatabaseName = "Contoso",
            ReplayMode = Enum.Parse<RepeatMode>(mode),
            SourceExecutionId = _original?.ExecutionId.ToString()
        };

        try
        {
            _repeat = await Sender.Send(command, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _rejection = ex;
        }
    }

    [Then("the repeat reports mode \"(.*)\"")]
    public void ThenTheRepeatReportsMode(string mode)
    {
        _repeat.ShouldNotBeNull();
        _repeat!.ReplayMode.ShouldBe(mode);
    }

    [Then("the repeat evaluation reports \"(.*)\"")]
    public void ThenTheRepeatEvaluationReports(string classification)
    {
        _repeat.ShouldNotBeNull();
        _repeat!.EvaluateSummary.Contains(classification, StringComparison.OrdinalIgnoreCase).ShouldBeTrue();
    }

    [Then("the repeat is linked to its source \"(.*)\"")]
    public void ThenTheRepeatIsLinkedToItsSource(bool linked)
    {
        _repeat.ShouldNotBeNull();
        if (linked)
        {
            _repeat!.SourceExecutionId.ShouldBe(_original!.ExecutionId.ToString());
        }
        else
        {
            _repeat!.SourceExecutionId.ShouldBeNull();
        }
    }

    [Then("the repeat reuses the recorded collect input \"(.*)\" \"(.*)\"")]
    public void ThenTheRepeatReusesTheRecordedCollectInput(string database, bool reuses)
    {
        _repeat.ShouldNotBeNull();
        _repeat!.CollectInput.ShouldBe(database);
        (_repeat.SourceExecutionId is not null).ShouldBe(reuses);
    }

    [Then("the repeat is rejected as \"(.*)\"")]
    public void ThenTheRepeatIsRejectedAs(string rejection)
    {
        _rejection.ShouldNotBeNull();
        _rejection!.GetType().Name.ShouldBe($"Custom{rejection}Exception");
    }

    private static PlaybookStepInput Step(string name, PlaybookActionFormat format, string definition) => new()
    {
        Name = name,
        Description = name,
        ActionFormat = format,
        ActionDefinition = definition
    };
}
