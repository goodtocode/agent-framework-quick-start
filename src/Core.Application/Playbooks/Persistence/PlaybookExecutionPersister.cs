namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Persistence;

/// <summary>
/// Shared helper so every example playbook's Run*PlaybookCommandHandler persists its execution the
/// same way: project the already-built <see cref="PlaybookExecutionResultDto"/> into a
/// <see cref="SavePlaybookExecutionCommand"/> and send it. Keeps the per-playbook handlers free of
/// duplicated persistence-mapping code.
/// </summary>
public static class PlaybookExecutionPersister
{
    public static Task<Guid> SaveAsync(ISender sender, PlaybookExecutionResultDto dto, CancellationToken cancellationToken) =>
        sender.Send(new SavePlaybookExecutionCommand
        {
            PlaybookKey = dto.PlaybookKey,
            PlaybookVersion = dto.Version,
            ReplayMode = dto.ReplayMode,
            SourceExecutionId = dto.SourceExecutionId,
            CollectInput = dto.CollectInput,
            CollectOutput = dto.CollectSummary,
            EvaluateOutput = dto.EvaluateSummary,
            RecordOutput = dto.RecordSummary,
            EvidenceJson = dto.EvidenceJson,
            FindingJson = dto.FindingJson,
            StartedUtc = dto.StartedUtc,
            CompletedUtc = dto.CompletedUtc
        }, cancellationToken);
}
