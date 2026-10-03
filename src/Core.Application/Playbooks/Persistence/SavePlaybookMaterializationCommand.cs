using Goodtocode.AgentFramework.Core.Domain.Playbooks;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Persistence;

/// <summary>
/// Persists a playbook's typed Record-stage materialization through a shared, generic
/// application command, so each example workflow (SQL Statistics, Taxonomy, Essay) can
/// optionally persist its result without a bespoke persistence schema. Intended to be called
/// from a Record-stage tool in <c>Infrastructure.AgentFramework</c>, never directly from a
/// playbook definition in <c>Core.Application</c>.
/// </summary>
public sealed class SavePlaybookMaterializationCommand : UserScopedRequest, IRequest<Guid>
{
    public required string PlaybookKey { get; init; }
    public required string PlaybookVersion { get; init; }
    public required string WorkflowType { get; init; }
    public required string SummaryText { get; init; }
    public required string PayloadSnapshot { get; init; }
}

public sealed class SavePlaybookMaterializationCommandHandler(IAgentFrameworkContext context) : IRequestHandler<SavePlaybookMaterializationCommand, Guid>
{
    private readonly IAgentFrameworkContext _context = context;

    public async Task<Guid> Handle(SavePlaybookMaterializationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = PlaybookMaterializationEntity.Create(
            ownerId: request.UserContext.OwnerId,
            tenantId: request.UserContext.TenantId,
            playbookKey: request.PlaybookKey,
            playbookVersion: request.PlaybookVersion,
            workflowType: request.WorkflowType,
            summaryText: request.SummaryText,
            payloadSnapshot: request.PayloadSnapshot);

        _context.PlaybookMaterializations.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
