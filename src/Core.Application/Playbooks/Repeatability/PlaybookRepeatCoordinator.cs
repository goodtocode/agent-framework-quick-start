using Goodtocode.AgentFramework.Core.Application.Playbooks.Persistence;
using Goodtocode.Agents.Governance.Domain;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.AgentFramework.Core.Application.Playbooks.Repeatability;

/// <summary>
/// Everything a Run*PlaybookCommand handler needs to execute a repeat: the Governance request, the Playbook
/// execution request carrying it and the prior state, and the source's collect input and record summary.
/// </summary>
public sealed record PreparedRepeat<TEvidence, TFinding>(
    RepeatRequest Request,
    PlaybookExecutionRequest<TEvidence, TFinding> ExecutionRequest,
    string? SourceCollectInput,
    string? SourceRecordSummary);

/// <summary>
/// Resolves the source execution for a repeat and builds the Playbook execution request. Which stages run is
/// decided by the Governance policy inside the Playbook executor, not here.
/// </summary>
public sealed class PlaybookRepeatCoordinator(IAgentFrameworkContext context)
{
    public async Task<PreparedRepeat<TEvidence, TFinding>> PrepareAsync<TEvidence, TFinding>(
        string playbookKey,
        IUserContext user,
        RepeatMode mode,
        string? sourceExecutionId,
        CancellationToken cancellationToken)
    {
        if (mode == RepeatMode.Rerun)
        {
            return new(RepeatRequest.Rerun(), PlaybookExecutionRequest<TEvidence, TFinding>.Default, null, null);
        }

        // Quick-start convention: an omitted source means "the user's latest execution".
        var source = await PlaybookReplaySourceResolver.ResolveAsync(
            context, playbookKey, user.OwnerId, user.TenantId, sourceExecutionId, cancellationToken);

        var request = RepeatRequest.Create(mode, source.Id.ToString());
        var executionRequest = new PlaybookExecutionRequest<TEvidence, TFinding>
        {
            Repeat = request,
            SeedEvidence = System.Text.Json.JsonSerializer.Deserialize<TEvidence>(source.EvidenceJson!),
            SeedFinding = System.Text.Json.JsonSerializer.Deserialize<TFinding>(source.FindingJson!)
        };

        return new(request, executionRequest, source.CollectInput, source.RecordOutput);
    }
}
