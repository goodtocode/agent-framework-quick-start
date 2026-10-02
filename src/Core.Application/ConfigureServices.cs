using Goodtocode.AgentFramework.Core.Application.Chats.Journeys;
using Goodtocode.AgentFramework.Core.Application.Common.Behaviors;
using Goodtocode.AgentFramework.Core.Application.Common.Idempotency;
using Goodtocode.AgentFramework.Core.Application.Governance;
using Goodtocode.AgentFramework.Core.Application.Playbooks;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Essay;
using Goodtocode.AgentFramework.Core.Application.Playbooks.SqlStatistics;
using Goodtocode.AgentFramework.Core.Application.Playbooks.Taxonomy;
using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Playbook.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.AgentFramework.Core.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatorServices();
        services.AddTransient(typeof(IPipelineBehavior<>), typeof(CustomUnhandledExceptionBehavior<>));
        services.AddTransient(typeof(IPipelineBehavior<>), typeof(CustomValidationBehavior<>));
        services.AddTransient(typeof(IPipelineBehavior<>), typeof(CustomPerformanceBehavior<>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CustomUnhandledExceptionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CustomValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CustomPerformanceBehavior<,>));
        services.AddValidationServices();
        services.AddSingleton<IIdempotencyDuplicateWindowPolicy, IdempotencyDuplicateWindowPolicy>();
        services.AddSingleton<ChatGovernanceGate>();
        services.AddSingleton<IChatSelectionTokenParser, ChatSelectionTokenParser>();

        services.AddSingleton<IRepeatabilityHashStrategy, DefaultRepeatabilityHashStrategy>();
        services.AddScoped<PlaybookGovernanceActivityRecorder>();
        services.AddScoped<DocumentReviewPlaybookDefinition>();
        services.AddSingleton<PlaybookExecutor<ReviewRequest, ReviewEvidence, ReviewFinding, ReviewRecord>>();

        services.AddScoped<SqlStatisticsGovernanceActivityRecorder>();
        services.AddScoped<SqlStatisticsPlaybookDefinition>();
        services.AddSingleton(SqlStatisticsKnowledgeHolder.V1);
        services.AddSingleton<PlaybookExecutor<string, SqlDatabaseStatisticsEvidence, SqlDatabaseSizeFinding, SqlDatabaseSizeMaterialization>>();
        services.AddScoped<ISqlStatisticsClassificationRunner, SqlStatisticsClassificationRunner>();

        services.AddScoped<TaxonomyGovernanceActivityRecorder>();
        services.AddSingleton(TaxonomyKnowledgeHolder.V1);

        services.AddScoped<EssayGovernanceActivityRecorder>();
        services.AddSingleton(EssayKnowledgeHolder.V1);

        return services;
    }
}