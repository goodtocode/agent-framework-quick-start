using Goodtocode.AgentFramework.Presentation.Web.Features.Playbooks.Models;
using Goodtocode.AgentFramework.Presentation.Web.Infrastructure.Http;

namespace Goodtocode.AgentFramework.Presentation.Web.Features.Playbooks.Services;

/// <summary>
/// Runs the three example Playbooks (SQL Statistics, Taxonomy, Essay) through the shared
/// <see cref="PlaybookExecutionResultModel"/> shape, mirroring the NSwag-generated
/// <c>RunMy*PlaybookAsync</c> client methods one-to-one so every playbook page can call this
/// service identically.
/// </summary>
public interface IPlaybookService
{
    Task<PlaybookExecutionResultModel> RunSqlStatisticsAsync(string databaseName);
    Task<PlaybookExecutionResultModel> RunTaxonomyAsync(string sourceText);
    Task<PlaybookExecutionResultModel> RunEssayAsync(string essayText);

    /// <summary>
    /// Fetches the persisted catalog definition (Name, Description, and the 3 CER steps'
    /// processing/action text) for the Playbook identified by <paramref name="key"/>.
    /// </summary>
    Task<PlaybookCatalogModel> GetCatalogAsync(string key);

    /// <summary>
    /// Fetches the current user's most recently completed execution of the Playbook identified
    /// by <paramref name="key"/>, or <c>null</c> if it has never been run.
    /// </summary>
    Task<PlaybookExecutionResultModel?> GetLatestExecutionAsync(string key);
}

public class PlaybookService(BackendApiClient client) : ApiService, IPlaybookService
{
    private readonly BackendApiClient _apiClient = client;

    public async Task<PlaybookExecutionResultModel> RunSqlStatisticsAsync(string databaseName)
    {
        var command = new RunSqlStatisticsPlaybookCommand { DatabaseName = databaseName };
        var response = await HandleApiException(() => _apiClient.RunMySqlStatisticsPlaybookAsync(command));

        return PlaybookExecutionResultModel.Create(response);
    }

    public async Task<PlaybookExecutionResultModel> RunTaxonomyAsync(string sourceText)
    {
        var command = new RunTaxonomyPlaybookCommand { Text = sourceText };
        var response = await HandleApiException(() => _apiClient.RunMyTaxonomyPlaybookAsync(command));

        return PlaybookExecutionResultModel.Create(response);
    }

    public async Task<PlaybookExecutionResultModel> RunEssayAsync(string essayText)
    {
        var command = new RunEssayPlaybookCommand { EssayText = essayText };
        var response = await HandleApiException(() => _apiClient.RunMyEssayPlaybookAsync(command));

        return PlaybookExecutionResultModel.Create(response);
    }

    public async Task<PlaybookCatalogModel> GetCatalogAsync(string key)
    {
        var response = await HandleApiException(() => _apiClient.GetPlaybookByKeyAsync(key));

        return PlaybookCatalogModel.Create(response);
    }

    public async Task<PlaybookExecutionResultModel?> GetLatestExecutionAsync(string key)
    {
        var response = await HandleApiExceptionOrDefault(() => _apiClient.GetMyLatestPlaybookExecutionAsync(key));

        return response is null ? null : PlaybookExecutionResultModel.Create(response);
    }
}
