using Goodtocode.AgentFramework.Core.Application.Common.Exceptions;
using Goodtocode.AgentFramework.Core.Domain.Playbooks;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Persistence;

/// <summary>
/// Resolves the prior <see cref="PlaybookExecutionEntity"/> a Recall or Replay request should run
/// against: either a specific execution by id, or - when no id is supplied - the current user's
/// most recently completed execution of that Playbook. Shared by every example playbook's
/// Run*PlaybookCommandHandler so "recall/replay the latest run" needs no per-playbook lookup code.
/// </summary>
public static class PlaybookReplaySourceResolver
{
    public static async Task<PlaybookExecutionEntity> ResolveAsync(
        IAgentFrameworkContext context,
        string playbookKey,
        Guid ownerId,
        Guid tenantId,
        string? sourceExecutionId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        var query = context.PlaybookExecutions
            .Where(x => x.PlaybookKey == playbookKey && x.OwnerId == ownerId && x.TenantId == tenantId);

        var entity = !string.IsNullOrWhiteSpace(sourceExecutionId) && Guid.TryParse(sourceExecutionId, out var id)
            ? await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            : await query.OrderByDescending(x => x.CompletedUtc).FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            throw new CustomNotFoundException(
                $"No prior execution of playbook '{playbookKey}' was found to recall or replay.");
        }

        if (string.IsNullOrWhiteSpace(entity.EvidenceJson) || string.IsNullOrWhiteSpace(entity.FindingJson))
        {
            throw new CustomConflictException(
                $"Execution '{entity.Id}' of playbook '{playbookKey}' does not have recorded evidence/finding and cannot be recalled or replayed.");
        }

        return entity;
    }
}
