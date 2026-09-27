using Goodtocode.AgentFramework.Core.Application.Playbooks;
using Goodtocode.Agents.Playbook.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks;

/// <summary>
/// Discovers and registers the Document Review example playbook's Collect/Evaluate/Record tools,
/// and registers the keyed resolver used to select a stage's tool per execution.
/// </summary>
public static class DocumentReviewPlaybookToolRegistration
{
    public static readonly PlaybookToolKey DefaultCollectKey = PlaybookToolKey.Create("document-review.deterministic.collect");

    public static readonly PlaybookToolKey DefaultEvaluateKey = PlaybookToolKey.Create("document-review.deterministic.evaluate");

    public static readonly PlaybookToolKey DefaultRecordKey = PlaybookToolKey.Create("document-review.deterministic.record");

    public static IServiceCollection AddDocumentReviewPlaybookTools(this IServiceCollection services)
    {
        services.AddPlaybookStepTools<ReviewRequest, ReviewEvidence, ReviewEvidence, ReviewFinding, ReviewFinding, ReviewRecord>(
            typeof(DocumentReviewPlaybookToolRegistration).Assembly);

        services.AddScoped<IPlaybookStepToolResolver<ReviewRequest, ReviewEvidence, ReviewEvidence, ReviewFinding, ReviewFinding, ReviewRecord>>(
            provider => new KeyedPlaybookStepToolResolver<ReviewRequest, ReviewEvidence, ReviewEvidence, ReviewFinding, ReviewFinding, ReviewRecord>(
                provider,
                DefaultCollectKey,
                DefaultEvaluateKey,
                DefaultRecordKey));

        return services;
    }
}
