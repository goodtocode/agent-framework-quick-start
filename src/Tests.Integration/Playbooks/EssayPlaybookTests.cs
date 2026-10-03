using Goodtocode.AgentFramework.Core.Application.Playbooks.Essay;
using Goodtocode.AgentFramework.Core.Domain.Chats;
using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.AgentFramework.Tests.Integration.Playbooks;

[TestClass]
public sealed class EssayPlaybookTests : TestBase
{
    [TestMethod]
    public void AddEssayPlaybookToolsDiscoversToolsByAttribute()
    {
        var resolver = ServiceProvider
            .GetRequiredService<IPlaybookStepToolResolver<string, EssayEvidence, EssayEvidence, EssayRubricFinding, EssayRubricFinding, EssayScorecardMaterialization>>();

        resolver.ResolveCollect(null).ToolName.ShouldBe("essay.deterministic.collect");
        resolver.ResolveEvaluate(null).ToolName.ShouldBe("essay.agentic.evaluate");
        resolver.ResolveRecord(null).ToolName.ShouldBe("essay.deterministic.record");
    }

    [TestMethod]
    public async Task EvaluateAsyncRunsDeterministicCollectAgenticEvaluateDeterministicRecordAndProducesGovernanceRecords()
    {
        agent.QueuedResponseTexts.Enqueue("""
            {"scores": [
                {"criterion": "Thesis clarity", "score": 0.9, "reason": "Clear thesis stated up front."},
                {"criterion": "Evidence and support", "score": 0.8, "reason": "Some supporting detail."},
                {"criterion": "Organization", "score": 0.85, "reason": "Logical flow."},
                {"criterion": "Grammar and mechanics", "score": 1.0, "reason": "No errors found."}
            ]}
            """);

        var runner = ServiceProvider.GetRequiredService<IEssayEvaluationRunner>();
        var recorder = ServiceProvider.GetRequiredService<EssayGovernanceActivityRecorder>();

        var result = await runner.EvaluateAsync(
            "This essay argues that renewable energy investment reduces long-term costs.", CancellationToken.None);

        result.Evidence.EssayText.ShouldBe("This essay argues that renewable energy investment reduces long-term costs.");
        result.Finding.CriterionScores.Count.ShouldBe(4);
        // 0.9*0.25 + 0.8*0.35 + 0.85*0.25 + 1.0*0.15 = 0.8675
        Math.Round(result.Finding.OverallScore, 4).ShouldBe(0.8675);
        result.Materialization.Grade.ShouldBe("B");

        recorder.Records.Count.ShouldBe(3);
        recorder.Records[0].Repeatability.DeterministicReplaySupported.ShouldBeTrue();
        recorder.Records[1].Repeatability.DeterministicReplaySupported.ShouldBeFalse();
        recorder.Records[2].Repeatability.DeterministicReplaySupported.ShouldBeTrue();
    }

    [TestMethod]
    public async Task EvaluateEssayCommandResolvesEssayTextFromChatMessageBeforeRunningThePlaybook()
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
            content: "This essay argues that renewable energy investment reduces long-term costs.");
        chatSession.Messages.Add(chatMessage);
        context.ChatSessions.Add(chatSession);
        await context.SaveChangesAsync(CancellationToken.None);

        agent.QueuedResponseTexts.Enqueue("""
            {"scores": [
                {"criterion": "Thesis clarity", "score": 0.9, "reason": "Clear thesis stated up front."},
                {"criterion": "Evidence and support", "score": 0.8, "reason": "Some supporting detail."},
                {"criterion": "Organization", "score": 0.85, "reason": "Logical flow."},
                {"criterion": "Grammar and mechanics", "score": 1.0, "reason": "No errors found."}
            ]}
            """);

        var result = await Sender.Send(new EvaluateEssayCommand { ChatMessageId = chatMessage.Id }, CancellationToken.None);

        result.Evidence.EssayText.ShouldBe("This essay argues that renewable energy investment reduces long-term costs.");
        result.Materialization.Grade.ShouldBe("B");
    }
}
