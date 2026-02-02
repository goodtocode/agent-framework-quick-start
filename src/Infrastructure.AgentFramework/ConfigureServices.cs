using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework;

public static class ConfigureServices
{
    public static IServiceCollection AddAgentFrameworkOpenAIServices(this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddOptions<OpenAIOptions>()
        .Bind(configuration.GetSection(OpenAIOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register your tools (plugins)
        services.AddSingleton<ChatSessionsTool>();
        services.AddSingleton<ActorsTool>();
        services.AddSingleton<ChatMessagesTool>();

        services.AddSingleton<AIAgent>(provider =>
        {
            var agent = new AIAgent();
            // ToDo: configure tools, etc.
            return agent;
        });

        return services;
    }
}