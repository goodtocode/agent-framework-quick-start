namespace Goodtocode.AgentFramework.Core.Domain.Playbooks;

/// <summary>
/// Persisted shape of a playbook's typed Record-stage materialization, shared across all
/// example playbooks (SQL Statistics, Taxonomy, Essay) so no workflow needs a bespoke
/// persistence schema. The typed materialization payload is captured as a flat summary plus a
/// serialized snapshot so the exact Record output can be replayed for display without decoding
/// business meaning out of a JSON blob at the domain layer.
/// </summary>
public class PlaybookMaterializationEntity : SecuredEntity<PlaybookMaterializationEntity>
{
    public string PlaybookKey { get; private set; } = string.Empty;
    public string PlaybookVersion { get; private set; } = string.Empty;
    public string WorkflowType { get; private set; } = string.Empty;
    public string SummaryText { get; private set; } = string.Empty;
    public string PayloadSnapshot { get; private set; } = string.Empty;

    protected PlaybookMaterializationEntity() : base() { }

    private PlaybookMaterializationEntity(
        Guid id,
        string canonicalKey,
        Guid ownerId,
        Guid tenantId,
        Guid createdBy,
        DateTime createdOn,
        DateTimeOffset timestamp,
        string playbookKey,
        string playbookVersion,
        string workflowType,
        string summaryText,
        string payloadSnapshot)
        : base(id: id, partitionKey: tenantId.ToString(), rowKey: canonicalKey,
               ownerId: ownerId, tenantId: tenantId, createdBy: createdBy,
               createdOn: createdOn, timestamp: timestamp)
    {
        PlaybookKey = playbookKey;
        PlaybookVersion = playbookVersion;
        WorkflowType = workflowType;
        SummaryText = summaryText;
        PayloadSnapshot = payloadSnapshot;
    }

    public static PlaybookMaterializationEntity Create(
        Guid ownerId,
        Guid tenantId,
        string playbookKey,
        string playbookVersion,
        string workflowType,
        string summaryText,
        string payloadSnapshot)
    {
        return new PlaybookMaterializationEntity(
            id: Guid.NewGuid(),
            canonicalKey: Guid.NewGuid().ToString(),
            ownerId: ownerId,
            tenantId: tenantId,
            createdBy: ownerId,
            createdOn: DateTime.UtcNow,
            timestamp: DateTimeOffset.UtcNow,
            playbookKey: playbookKey,
            playbookVersion: playbookVersion,
            workflowType: workflowType,
            summaryText: summaryText,
            payloadSnapshot: payloadSnapshot);
    }
}
