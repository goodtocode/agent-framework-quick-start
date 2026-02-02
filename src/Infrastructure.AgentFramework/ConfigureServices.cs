using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        services.AddSingleton<ChatSessionsTool>();
        services.AddSingleton<ActorsTool>();
        services.AddSingleton<ChatMessagesTool>();

        services.AddSingleton<AIAgent>(provider =>
        {
            var chatClient = provider.GetRequiredService<IChatClient>();
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var tools = new List<AITool>
            {
                provider.GetRequiredService<ChatSessionsTool>(),
                provider.GetRequiredService<ActorsTool>(),
                provider.GetRequiredService<ChatMessagesTool>()
            };

            var agent = new ChatClientAgent(
                chatClient,
                instructions: "You are a helpful assistant.",
                name: "CopilotAgent",
                description: "Agent Framework Quick-start Copilot",
                tools: tools,
                loggerFactory: loggerFactory,
                services: provider
            );

            var builder = new AIAgentBuilder(agent);
            return builder.Build(provider);
        });

        return services;
    }
}