using Goodtocode.AgentFramework.Core.Application.Common.Behaviors;
using Goodtocode.AgentFramework.Core.Application.Governance;
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
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CustomPerformanceBehavior<,>));
        services.AddValidationServices();
        services.AddSingleton<ChatGovernanceGate>();

        return services;
    }
}