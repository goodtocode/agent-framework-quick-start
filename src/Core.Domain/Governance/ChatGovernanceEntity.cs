namespace Goodtocode.AgentFramework.Core.Domain.Governance;

public class ChatGovernanceEntity : SecuredEntity<ChatGovernanceEntity>
{
    public Guid ChatSessionId { get; private set; }
    public string PolicyProfileVersion { get; private set; } = string.Empty;
    public string TraceId { get; private set; } = string.Empty;
    public Guid CorrelationId { get; private set; }
    public string PrincipalDisplay { get; private set; } = string.Empty;
    public string ModelRef { get; private set; } = string.Empty;
    public string ModelVersion { get; private set; } = string.Empty;
    public string PromptHash { get; private set; } = string.Empty;
    public string InputHash { get; private set; } = string.Empty;
    public bool DeterministicReplaySupported { get; private set; }
    public string SystemInstruction { get; private set; } = string.Empty;
    public string MetadataJson { get; private set; } = string.Empty;
    public string EvidenceRefsJson { get; private set; } = string.Empty;
    public string ToolRefsJson { get; private set; } = string.Empty;
    public string PoliciesAppliedJson { get; private set; } = string.Empty;
    public string JustificationRefsJson { get; private set; } = string.Empty;
    public string ReasoningSummary { get; private set; } = string.Empty;
    public decimal? ConfidenceScore { get; private set; }

    protected ChatGovernanceEntity() : base() { }

    private ChatGovernanceEntity(
        Guid id,
        string canonicalKey,
        Guid ownerId,
        Guid tenantId,
        Guid createdBy,
        DateTime createdOn,
        DateTimeOffset timestamp,
        Guid chatSessionId,
        string policyProfileVersion,
        string traceId,
        Guid correlationId,
        string principalDisplay,
        string modelRef,
        string modelVersion,
        string promptHash,
        string inputHash,
        bool deterministicReplaySupported,
        string systemInstruction,
        string metadataJson,
        string evidenceRefsJson,
        string toolRefsJson,
        string policiesAppliedJson,
        string justificationRefsJson,
        string reasoningSummary,
        decimal? confidenceScore)
        : base(id, tenantId.ToString(), canonicalKey, ownerId, tenantId, createdBy, createdOn, timestamp)
    {
        ChatSessionId = chatSessionId;
        PolicyProfileVersion = policyProfileVersion;
        TraceId = traceId;
        CorrelationId = correlationId;
        PrincipalDisplay = principalDisplay;
        ModelRef = modelRef;
        ModelVersion = modelVersion;
        PromptHash = promptHash;
        InputHash = inputHash;
        DeterministicReplaySupported = deterministicReplaySupported;
        SystemInstruction = systemInstruction;
        MetadataJson = metadataJson;
        EvidenceRefsJson = evidenceRefsJson;
        ToolRefsJson = toolRefsJson;
        PoliciesAppliedJson = policiesAppliedJson;
        JustificationRefsJson = justificationRefsJson;
        ReasoningSummary = reasoningSummary;
        ConfidenceScore = confidenceScore;
    }

    public static ChatGovernanceEntity Create(
        Guid ownerId,
        Guid tenantId,
        Guid chatSessionId,
        string policyProfileVersion,
        string traceId,
        Guid correlationId,
        string principalDisplay,
        string modelRef,
        string modelVersion,
        string promptHash,
        string inputHash,
        bool deterministicReplaySupported,
        string systemInstruction,
        string metadataJson,
        string evidenceRefsJson,
        string toolRefsJson,
        string policiesAppliedJson,
        string justificationRefsJson,
        string reasoningSummary,
        decimal? confidenceScore)
    {
        return new ChatGovernanceEntity(
            Guid.NewGuid(),
            Guid.NewGuid().ToString(),
            ownerId,
            tenantId,
            ownerId,
            DateTime.UtcNow,
            DateTimeOffset.UtcNow,
            chatSessionId,
            policyProfileVersion,
            traceId,
            correlationId,
            principalDisplay,
            modelRef,
            modelVersion,
            promptHash,
            inputHash,
            deterministicReplaySupported,
            systemInstruction,
            metadataJson,
            evidenceRefsJson,
            toolRefsJson,
            policiesAppliedJson,
            justificationRefsJson,
            reasoningSummary,
            confidenceScore);
    }
}