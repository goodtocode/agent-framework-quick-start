using System.Text.Json;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Essay;
using Goodtocode.AgentFramework.Core.Domain.Chats;

namespace Goodtocode.AgentFramework.Tests.Integration.Playbooks;

[Binding]
[Scope(Tag = "essayPlaybook")]
public sealed class EssayPlaybookStepDefinitions : TestBase
{
    private Guid _chatMessageId;
    private Goodtocode.Agents.Playbook.Execution.PlaybookExecutionResult<EssayEvidence, EssayRubricFinding, EssayScorecardMaterialization>? _result;
    private EssayGovernanceActivityRecorder? _recorder;

    [Given("an essay submission chat message with content \"(.*)\"")]
    public async Task GivenAnEssaySubmissionChatMessageWithContent(string content)
    {
        var chatSession = ChatSessionEntity.Create(
            ownerId: rlsContext.OwnerId,
            tenantId: rlsContext.TenantId,
            actorId: Guid.NewGuid(),
            title: "Essay Submission Session");
        var chatMessage = ChatMessageEntity.Create(
            ownerId: rlsContext.OwnerId,
            tenantId: rlsContext.TenantId,
            chatSessionId: chatSession.Id,
            role: ChatMessageRole.user,
            content: content);
        chatSession.Messages.Add(chatMessage);
        context.ChatSessions.Add(chatSession);
        await context.SaveChangesAsync(CancellationToken.None);

        _chatMessageId = chatMessage.Id;
    }

    [Given("the model scores the essay as:")]
    public void GivenTheModelScoresTheEssayAs(Table table)
    {
        var scores = table.Rows.Select(row => new
        {
            criterion = row["criterion"],
            score = double.Parse(row["score"], System.Globalization.CultureInfo.InvariantCulture),
            reason = row["reason"]
        });

        agent.QueuedResponseTexts.Enqueue(JsonSerializer.Serialize(new { scores }));
    }

    [When("I execute the essay playbook for that submission")]
    public async Task WhenIExecuteTheEssayPlaybookForThatSubmission()
    {
        _recorder = ServiceProvider.GetRequiredService<EssayGovernanceActivityRecorder>();

        _result = await Sender.Send(new EvaluateEssayCommand { ChatMessageId = _chatMessageId }, CancellationToken.None);
    }

    [Then("the essay grade is \"(.*)\"")]
    public void ThenTheEssayGradeIs(string grade)
    {
        _result.ShouldNotBeNull();
        _result!.Materialization.Grade.ShouldBe(grade);
    }

    [Then("(.*) governance records are produced for the essay playbook")]
    public void ThenGovernanceRecordsAreProducedForTheEssayPlaybook(int expectedCount)
    {
        _recorder.ShouldNotBeNull();
        _recorder!.Records.Count.ShouldBe(expectedCount);
    }
}
