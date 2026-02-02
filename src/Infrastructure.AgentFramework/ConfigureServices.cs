using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

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

        services.AddSingleton<ChatClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            return new ChatClient(options.ChatCompletionModelId, options.ApiKey);
        });

        services.AddSingleton<AIAgent>(provider =>
        {
            var chatClient = provider.GetRequiredService<ChatClient>();
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var tools = new List<AITool>
            {
                provider.GetRequiredService<ChatSessionsTool>(),
                provider.GetRequiredService<ActorsTool>(),
                provider.GetRequiredService<ChatMessagesTool>()
            };

            var agentOptions = new ChatClientAgentOptions
            {                 
                Name = "CopilotAgent",
                Description = "Microsoft Agent Framework Quick-start Copilot",
            };

            return chatClient.AsAIAgent(
                options: agentOptions,
                clientFactory: null,
                loggerFactory: loggerFactory,
                services: provider
            );
        });

        return services;
    }
}