using Goodtocode.AgentFramework.Core.Domain.Playbooks;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Persistence;

/// <summary>
/// Persists one actual run of a Playbook - the user's Collect-stage input plus each stage's
/// output - through a shared, generic application command, so each example workflow (SQL
/// Statistics, Taxonomy, Essay) persists its result without a bespoke persistence schema. Called
/// from each example playbook's Run*PlaybookCommand handler in <c>Core.Application</c> once the
/// Playbook definition has been resolved, so every execution is recorded against its Playbook via
/// <see cref="Core.Domain.Playbooks.PlaybookExecutionEntity.PlaybookId"/>.
/// </summary>
public sealed class SavePlaybookExecutionCommand : UserScopedRequest, IRequest<Guid>
{
    public required string PlaybookKey { get; init; }
    public required string PlaybookVersion { get; init; }
    public required string ReplayMode { get; init; }
    public string? SourceExecutionId { get; init; }
    public required string CollectInput { get; init; }
    public required string CollectOutput { get; init; }
    public required string EvaluateOutput { get; init; }
    public required string RecordOutput { get; init; }
    public string? EvidenceJson { get; init; }
    public string? FindingJson { get; init; }
    public required DateTimeOffset StartedUtc { get; init; }
    public required DateTimeOffset CompletedUtc { get; init; }
}

public sealed class SavePlaybookExecutionCommandHandler(IAgentFrameworkContext context) : IRequestHandler<SavePlaybookExecutionCommand, Guid>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<Guid> Handle(SavePlaybookExecutionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var playbook = await _context.Playbooks.FirstOrDefaultAsync(x => x.Key == request.PlaybookKey, cancellationToken)
            ?? throw new CustomNotFoundException($"Playbook '{request.PlaybookKey}' was not found; it must be seeded before executions can be saved.");

        var entity = PlaybookExecutionEntity.Create(
            ownerId: request.UserContext.OwnerId,
            tenantId: request.UserContext.TenantId,
            playbookId: playbook.Id,
            playbookKey: request.PlaybookKey,
            playbookVersion: request.PlaybookVersion,
            workflowType: playbook.WorkflowType,
            replayMode: request.ReplayMode,
            sourceExecutionId: request.SourceExecutionId,
            collectInput: request.CollectInput,
            collectOutput: request.CollectOutput,
            evaluateOutput: request.EvaluateOutput,
            recordOutput: request.RecordOutput,
            evidenceJson: request.EvidenceJson,
            findingJson: request.FindingJson,
            startedUtc: request.StartedUtc,
            completedUtc: request.CompletedUtc);

        _context.PlaybookExecutions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
