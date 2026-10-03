namespace Goodtocode.AgentFramework.Core.Domain.Playbooks;

/// <summary>
/// Persisted record of one actual run of a <see cref="PlaybookEntity"/>: the user-supplied
/// Collect-stage input plus each stage's output (Collect/Evaluate/Record), all as plain strings
/// per the shared CER input/output convention every example playbook follows. Replaces the
/// previous, never-persisted <c>PlaybookMaterializationEntity</c> - this is the "what happened"
/// half of the Playbook concept, separate from the "what it is" half captured by
/// <see cref="PlaybookEntity"/> and <see cref="PlaybookStepEntity"/>.
/// </summary>
public class PlaybookExecutionEntity : SecuredEntity<PlaybookExecutionEntity>
{
    public Guid PlaybookId { get; private set; }
    public string PlaybookKey { get; private set; } = string.Empty;
    public string PlaybookVersion { get; private set; } = string.Empty;
    public string WorkflowType { get; private set; } = string.Empty;
    public string ReplayMode { get; private set; } = string.Empty;
    public string? SourceExecutionId { get; private set; }
    public string CollectInput { get; private set; } = string.Empty;
    public string CollectOutput { get; private set; } = string.Empty;
    public string EvaluateOutput { get; private set; } = string.Empty;
    public string RecordOutput { get; private set; } = string.Empty;
    public DateTimeOffset StartedUtc { get; private set; }
    public DateTimeOffset CompletedUtc { get; private set; }
    public virtual PlaybookEntity? Playbook { get; private set; }

    protected PlaybookExecutionEntity() : base() { }

    private PlaybookExecutionEntity(
        Guid id,
        string canonicalKey,
        Guid ownerId,
        Guid tenantId,
        Guid createdBy,
        DateTime createdOn,
        DateTimeOffset timestamp,
        Guid playbookId,
        string playbookKey,
        string playbookVersion,
        string workflowType,
        string replayMode,
        string? sourceExecutionId,
        string collectInput,
        string collectOutput,
        string evaluateOutput,
        string recordOutput,
        DateTimeOffset startedUtc,
        DateTimeOffset completedUtc)
        : base(id: id, partitionKey: tenantId.ToString(), rowKey: canonicalKey,
               ownerId: ownerId, tenantId: tenantId, createdBy: createdBy,
               createdOn: createdOn, timestamp: timestamp)
    {
        PlaybookId = playbookId;
        PlaybookKey = playbookKey;
        PlaybookVersion = playbookVersion;
        WorkflowType = workflowType;
        ReplayMode = replayMode;
        SourceExecutionId = sourceExecutionId;
        CollectInput = collectInput;
        CollectOutput = collectOutput;
        EvaluateOutput = evaluateOutput;
        RecordOutput = recordOutput;
        StartedUtc = startedUtc;
        CompletedUtc = completedUtc;
    }

    public static PlaybookExecutionEntity Create(
        Guid ownerId,
        Guid tenantId,
        Guid playbookId,
        string playbookKey,
        string playbookVersion,
        string workflowType,
        string replayMode,
        string? sourceExecutionId,
        string collectInput,
        string collectOutput,
        string evaluateOutput,
        string recordOutput,
        DateTimeOffset startedUtc,
        DateTimeOffset completedUtc)
    {
        return new PlaybookExecutionEntity(
            id: Guid.NewGuid(),
            canonicalKey: Guid.NewGuid().ToString(),
            ownerId: ownerId,
            tenantId: tenantId,
            createdBy: ownerId,
            createdOn: DateTime.UtcNow,
            timestamp: DateTimeOffset.UtcNow,
            playbookId: playbookId,
            playbookKey: playbookKey,
            playbookVersion: playbookVersion,
            workflowType: workflowType,
            replayMode: replayMode,
            sourceExecutionId: sourceExecutionId,
            collectInput: collectInput,
            collectOutput: collectOutput,
            evaluateOutput: evaluateOutput,
            recordOutput: recordOutput,
            startedUtc: startedUtc,
            completedUtc: completedUtc);
    }
}
