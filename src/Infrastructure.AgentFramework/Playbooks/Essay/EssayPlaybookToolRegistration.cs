using Goodtocode.AgentFramework.Core.Application.Playbooks.Essay;
using Goodtocode.Agents.Playbook.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Playbooks.Essay;

/// <summary>
/// Discovers and registers the Essay Rubric Evaluation example playbook's Collect/Evaluate/Record
/// tools, and registers the keyed resolver used to select a stage's tool per execution.
/// </summary>
public static class EssayPlaybookToolRegistration
{
    public static readonly PlaybookToolKey DefaultCollectKey = PlaybookToolKey.Create("essay.deterministic.collect");

    public static readonly PlaybookToolKey DefaultEvaluateKey = PlaybookToolKey.Create("essay.agentic.evaluate");

    public static readonly PlaybookToolKey DefaultRecordKey = PlaybookToolKey.Create("essay.deterministic.record");

    public static IServiceCollection AddEssayPlaybookTools(this IServiceCollection services)
    {
        services.AddPlaybookStepTools<string, EssayEvidence, EssayEvidence, EssayRubricFinding, EssayRubricFinding, EssayScorecardMaterialization>(
            typeof(EssayPlaybookToolRegistration).Assembly);

        services.AddScoped<IPlaybookStepToolResolver<string, EssayEvidence, EssayEvidence, EssayRubricFinding, EssayRubricFinding, EssayScorecardMaterialization>>(
            provider => new KeyedPlaybookStepToolResolver<string, EssayEvidence, EssayEvidence, EssayRubricFinding, EssayRubricFinding, EssayScorecardMaterialization>(
                provider,
                DefaultCollectKey,
                DefaultEvaluateKey,
                DefaultRecordKey));

        return services;
    }
}
