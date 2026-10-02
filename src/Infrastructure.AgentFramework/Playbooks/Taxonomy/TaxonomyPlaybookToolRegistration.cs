using Goodtocode.AgentFramework.Core.Application.Playbooks.Taxonomy;
using Goodtocode.Agents.Playbook.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks.Taxonomy;

/// <summary>
/// Discovers and registers the Taxonomy Extraction and Classification example playbook's
/// Collect/Evaluate/Record agentic tools, and registers the keyed resolver used to select a
/// stage's tool per execution.
/// </summary>
public static class TaxonomyPlaybookToolRegistration
{
    public static readonly PlaybookToolKey DefaultCollectKey = PlaybookToolKey.Create("taxonomy.agentic.collect");

    public static readonly PlaybookToolKey DefaultEvaluateKey = PlaybookToolKey.Create("taxonomy.agentic.evaluate");

    public static readonly PlaybookToolKey DefaultRecordKey = PlaybookToolKey.Create("taxonomy.agentic.record");

    public static IServiceCollection AddTaxonomyPlaybookTools(this IServiceCollection services)
    {
        services.AddPlaybookStepTools<string, TaxonomyEvidence, TaxonomyEvidence, TaxonomyFinding, TaxonomyFinding, TaxonomyMaterialization>(
            typeof(TaxonomyPlaybookToolRegistration).Assembly);

        services.AddScoped<IPlaybookStepToolResolver<string, TaxonomyEvidence, TaxonomyEvidence, TaxonomyFinding, TaxonomyFinding, TaxonomyMaterialization>>(
            provider => new KeyedPlaybookStepToolResolver<string, TaxonomyEvidence, TaxonomyEvidence, TaxonomyFinding, TaxonomyFinding, TaxonomyMaterialization>(
                provider,
                DefaultCollectKey,
                DefaultEvaluateKey,
                DefaultRecordKey));

        return services;
    }
}
