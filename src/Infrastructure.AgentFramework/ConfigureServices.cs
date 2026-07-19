using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Providers;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework;

public static class ConfigureServices
{
    public static IServiceCollection AddAgentFrameworkOpenAIServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddWebSearchProviders(configuration);

        services.AddOptions<AgentProviderOptions>()
            .Bind(configuration.GetSection(AgentProviderOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<OpenAIOptions>()
            .Bind(configuration.GetSection(OpenAIOptions.SectionName))
            .Validate<IOptions<AgentProviderOptions>>(
                (options, provider) => !IsSelectedProvider(provider.Value, "OpenAI") || HasRequiredOpenAI(options),
                "OpenAI options are required when AgentProvider:Kind is OpenAI.")
            .ValidateOnStart();

        services.AddOptions<GitHubCopilotSdkOptions>()
            .Bind(configuration.GetSection(GitHubCopilotSdkOptions.SectionName))
            .Validate<IOptions<AgentProviderOptions>>(
                (options, provider) => !IsSelectedProvider(provider.Value, "GitHubCopilotSDK") || HasRequiredGitHubCopilotSdk(options),
                "GitHubCopilotSDK options are required when AgentProvider:Kind is GitHubCopilotSDK.")
            .ValidateOnStart();

        services.AddOptions<AzureOpenAIOptions>()
            .Bind(configuration.GetSection(AzureOpenAIOptions.SectionName))
            .Validate<IOptions<AgentProviderOptions>>(
                (options, provider) => !IsSelectedProvider(provider.Value, "AzureOpenAI") || HasRequiredAzureOpenAI(options),
                "AzureOpenAI options are required when AgentProvider:Kind is AzureOpenAI.")
            .ValidateOnStart();

        services.AddOptions<AzureAIFoundryOptions>()
            .Bind(configuration.GetSection(AzureAIFoundryOptions.SectionName))
            .Validate<IOptions<AgentProviderOptions>>(
                (options, provider) => !IsSelectedProvider(provider.Value, "AzureAIFoundry") || HasRequiredAzureAIFoundry(options),
                "AzureAIFoundry options are required when AgentProvider:Kind is AzureAIFoundry.")
            .ValidateOnStart();

        services.AddOptions<CopilotKnowledgeEnrichmentOptions>()
            .Bind(configuration.GetSection(CopilotKnowledgeEnrichmentOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ChatSessionsTool>();
        services.AddSingleton<ActorsTool>();
        services.AddSingleton<ChatMessagesTool>();
        services.AddSingleton<WebSearchTool>();

        services.AddSingleton(provider =>
        {
            var providerOptions = provider.GetRequiredService<IOptions<AgentProviderOptions>>().Value;

            if (providerOptions.Kind.Equals("GitHubCopilotSDK", StringComparison.OrdinalIgnoreCase))
            {
                var options = provider.GetRequiredService<IOptions<GitHubCopilotSdkOptions>>().Value;
                var client = new OpenAIClient(
                    new System.ClientModel.ApiKeyCredential(options.ApiKey),
                    new OpenAIClientOptions
                    {
                        Endpoint = new Uri(options.Endpoint)
                    });
                return client.GetChatClient(options.ChatCompletionModelId);
            }

            if (providerOptions.Kind.Equals("AzureOpenAI", StringComparison.OrdinalIgnoreCase))
            {
                var options = provider.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;
                var client = new OpenAIClient(
                    new System.ClientModel.ApiKeyCredential(options.ApiKey),
                    new OpenAIClientOptions
                    {
                        Endpoint = new Uri(options.Endpoint)
                    });
                return client.GetChatClient(options.ChatDeploymentName);
            }

            if (providerOptions.Kind.Equals("AzureAIFoundry", StringComparison.OrdinalIgnoreCase))
            {
                var options = provider.GetRequiredService<IOptions<AzureAIFoundryOptions>>().Value;
                var client = new OpenAIClient(
                    new System.ClientModel.ApiKeyCredential(options.ApiKey),
                    new OpenAIClientOptions
                    {
                        Endpoint = new Uri(options.Endpoint)
                    });
                return client.GetChatClient(options.ChatCompletionModelId);
            }

            var openAIOptions = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            return new ChatClient(openAIOptions.ChatCompletionModelId, openAIOptions.ApiKey);
        });

        services.AddSingleton<AIAgent>(provider =>
        {
            var chatClient = provider.GetRequiredService<ChatClient>();
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var tools = new List<AITool>
            {
                provider.GetRequiredService<ChatSessionsTool>(),
                provider.GetRequiredService<ActorsTool>(),
                provider.GetRequiredService<ChatMessagesTool>(),
                provider.GetRequiredService<WebSearchTool>()
            };

            var agentOptions = new ChatClientAgentOptions
            {
                Name = "CopilotAgent",
                Description = "GoodToCode AgentFramework Copilot",
                ChatOptions = new Microsoft.Extensions.AI.ChatOptions
                {
                    Tools = tools
                }
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

    public static IServiceCollection AddWebSearchProviders(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SearchProviderOptions>()
            .Bind(configuration.GetSection(SearchProviderOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                HasRequiredSearchProviderOptions,
                "SearchProvider options are invalid for the selected search provider.")
            .ValidateOnStart();

        services.AddScoped<NoOpWebSearchProvider>();

        services.AddHttpClient<BingSearchProvider>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<SearchProviderOptions>>().Value;
            if (Uri.TryCreate(options.Endpoint, UriKind.Absolute, out var endpoint))
            {
                client.BaseAddress = endpoint;
            }

            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IWebSearchProvider>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<SearchProviderOptions>>().Value;
            if (options.Provider.Equals("Bing", StringComparison.OrdinalIgnoreCase)
                && IsBingConfigured(options))
            {
                return provider.GetRequiredService<BingSearchProvider>();
            }

            return provider.GetRequiredService<NoOpWebSearchProvider>();
        });

        return services;
    }

    private static bool IsSelectedProvider(AgentProviderOptions options, string providerKind)
        => options.Kind.Equals(providerKind, StringComparison.OrdinalIgnoreCase);

    private static bool HasRequiredOpenAI(OpenAIOptions options)
        => HasValue(options.ChatCompletionModelId)
           && HasValue(options.TextGenerationModelId)
           && HasValue(options.TextEmbeddingModelId)
           && HasValue(options.TextModerationModelId)
           && HasValue(options.ImageModelId)
           && HasValue(options.AudioModelId)
           && HasValue(options.ApiKey);

    private static bool HasRequiredGitHubCopilotSdk(GitHubCopilotSdkOptions options)
        => HasValue(options.ChatCompletionModelId)
           && HasValue(options.TextGenerationModelId)
           && HasValue(options.TextEmbeddingModelId)
           && HasValue(options.TextModerationModelId)
           && HasValue(options.ImageModelId)
           && HasValue(options.AudioModelId)
           && HasValue(options.ApiKey)
           && HasUri(options.Endpoint);

    private static bool HasRequiredAzureOpenAI(AzureOpenAIOptions options)
        => HasValue(options.ChatDeploymentName)
           && HasValue(options.ApiKey)
           && HasUri(options.Endpoint);

    private static bool HasRequiredAzureAIFoundry(AzureAIFoundryOptions options)
        => HasValue(options.ChatCompletionModelId)
           && HasValue(options.TextGenerationModelId)
           && HasValue(options.TextEmbeddingModelId)
           && HasValue(options.TextModerationModelId)
           && HasValue(options.ImageModelId)
           && HasValue(options.AudioModelId)
           && HasValue(options.ApiKey)
           && HasUri(options.Endpoint);

    private static bool HasRequiredSearchProviderOptions(SearchProviderOptions options)
    {
        if (!HasValue(options.Provider))
        {
            return false;
        }

        if (options.Provider.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            return options.DefaultResultCount > 0;
        }

        if (options.Provider.Equals("Bing", StringComparison.OrdinalIgnoreCase))
        {
            return options.DefaultResultCount > 0;
        }

        return false;
    }

    private static bool IsBingConfigured(SearchProviderOptions options)
    {
        var apiKey = options.ApiKey ?? string.Empty;
        if (!HasValue(apiKey)
            || apiKey.Equals("<secret>", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return HasUri(options.Endpoint ?? string.Empty);
    }

    private static bool HasValue(string value)
        => !string.IsNullOrWhiteSpace(value);

    private static bool HasUri(string value)
        => Uri.TryCreate(value, UriKind.Absolute, out _);
}
