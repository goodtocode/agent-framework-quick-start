using Goodtocode.AgentStart.Core.Application.Chats.Journeys;
using Goodtocode.AgentStart.Core.Application.Governance;
using Goodtocode.AgentStart.Core.Application.Playbooks.Essay;
using Goodtocode.AgentStart.Core.Application.Playbooks.Taxonomy;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Embeddings;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Execution;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Governance;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Intents;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Journeys;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Options;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Playbooks;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Playbooks.Essay;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Playbooks.SqlStatistics;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Playbooks.Taxonomy;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Providers;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Tools;
using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Playbook.Prompting.Governance;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace Goodtocode.AgentStart.Infrastructure.AgentFramework;

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

        services.AddOptions<OllamaOptions>()
            .Bind(configuration.GetSection(OllamaOptions.SectionName))
            .Validate<IOptions<AgentProviderOptions>>(
                (options, provider) => !IsSelectedProvider(provider.Value, "Ollama") || HasRequiredOllama(options),
                "Ollama options are required when AgentProvider:Kind is Ollama.")
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

        services.AddOptions<AgentToolInstructionsOptions>()
            .Bind(configuration.GetSection(AgentToolInstructionsOptions.SectionName));
        services.AddOptions<IntentClassificationOptions>()
            .Bind(configuration.GetSection(IntentClassificationOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IAgentInstructionsComposer, AgentInstructionsComposer>();

        services.AddOptions<GovernedIntentOptions>()
            .Bind(configuration.GetSection(GovernedIntentOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<ChatGovernanceScope>();
        services.AddSingleton<IGovernedIntentScope>(sp => sp.GetRequiredService<ChatGovernanceScope>());
        services.AddSingleton<IIntentPolicyProvider>(new StaticIntentPolicyProvider(SampleIntentPolicy.Create()));
        services.AddGovernedIntentPreamble();

        services.AddScoped<IToolApplicationExecutor, ToolApplicationExecutor>();

        var searchProviderOptions = configuration.GetSection(SearchProviderOptions.SectionName).Get<SearchProviderOptions>()
            ?? new SearchProviderOptions();
        services.AddSingleton(DefaultIntentCatalogFactory.Create(searchProviderOptions.IsAvailable));
        services.AddSingleton<RuleIntentClassifier>();
        services.AddScoped<SemanticIntentClassifier>();
        services.AddScoped<IIntentClassifier, HybridIntentClassifier>();
        services.AddHostedService<IntentEmbeddingInitializationService>();
        services.AddHostedService<PlaybookCatalogSeedInitializationService>();
        services.AddSingleton<IAgentChatContextAccessor, AgentChatContextAccessor>();
        services.AddSingleton<IChatJourneyCatalogContributor, DefaultChatJourneyCatalogContributor>();
        services.AddSingleton<ChatJourneyCatalogFactory>();
        services.AddSingleton<IChatJourneyProvider, RuleChatJourneyProvider>();
        services.AddSingleton<IChatJourneyContextAccessor, AgentChatJourneyContextAccessor>();
        services.AddScoped<ChatGovernanceGate>();
        services.AddScoped<ChatMessageIntentRouter>();
        services.AddScoped<IChatMessageRouter>(provider => provider.GetRequiredService<ChatMessageIntentRouter>());
        services.AddScoped<IIntentRouter>(provider => provider.GetRequiredService<ChatMessageIntentRouter>());

        services.AddSingleton<MyChatSessionsTool>();
        services.AddSingleton<ActorsTool>();
        services.AddSingleton<MyChatMessagesTool>();
        services.AddSingleton<WebSearchTool>();

        services.AddDocumentReviewPlaybookTools();
        services.AddSqlStatisticsPlaybookTools();
        services.AddTaxonomyPlaybookTools();
        services.AddScoped<ITaxonomyClassificationRunner, TaxonomyClassificationRunner>();
        services.AddEssayPlaybookTools();
        services.AddScoped<IEssayEvaluationRunner, EssayEvaluationRunner>();

        foreach (var purpose in Enum.GetValues<InferencePurpose>())
        {
            services.AddKeyedSingleton<ChatClient>(purpose, (provider, _) =>
            {
                var providerOptions = provider.GetRequiredService<IOptions<AgentProviderOptions>>().Value;
                var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("AgentProviderConfiguration");
                logger.LogInformation("Resolving {Provider} inference purpose {Purpose}", providerOptions.Kind, purpose);

                if (providerOptions.Kind.Equals("GitHubCopilotSDK", StringComparison.OrdinalIgnoreCase))
                {
                    var options = provider.GetRequiredService<IOptions<GitHubCopilotSdkOptions>>().Value;
                    logger.LogInformation("Agent provider resolved to {Provider}. Endpoint: {Endpoint}. Model: {Model}",
                        providerOptions.Kind,
                        options.Endpoint,
                        SelectModel(purpose, options.ChatCompletionModelId, options.ToolCompletionModelId, options.ToolReasoningModelId));

                    var client = new OpenAIClient(
                        new System.ClientModel.ApiKeyCredential(options.ApiKey),
                        CreateClientOptions(options.Endpoint));
                    return client.GetChatClient(SelectModel(purpose, options.ChatCompletionModelId, options.ToolCompletionModelId, options.ToolReasoningModelId));
                }

                if (providerOptions.Kind.Equals("AzureOpenAI", StringComparison.OrdinalIgnoreCase))
                {
                    var options = provider.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;
                    var normalizedEndpoint = NormalizeAzureOpenAIEndpoint(options.Endpoint);
                    logger.LogInformation("Agent provider resolved to {Provider}. Endpoint: {Endpoint}. NormalizedEndpoint: {NormalizedEndpoint}. Model: {Model}",
                        providerOptions.Kind,
                        options.Endpoint,
                        normalizedEndpoint,
                        SelectModel(purpose, options.ChatDeploymentName, options.ToolDeploymentName, options.ToolReasoningDeploymentName));

                    var client = new OpenAIClient(
                        new System.ClientModel.ApiKeyCredential(options.ApiKey),
                        CreateClientOptions(normalizedEndpoint));
                    return client.GetChatClient(SelectModel(purpose, options.ChatDeploymentName, options.ToolDeploymentName, options.ToolReasoningDeploymentName));
                }

                if (providerOptions.Kind.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
                {
                    var options = provider.GetRequiredService<IOptions<OllamaOptions>>().Value;
                    logger.LogInformation("Agent provider resolved to {Provider}. Endpoint: {Endpoint}. Model: {Model}",
                        providerOptions.Kind,
                        options.Endpoint,
                        SelectModel(purpose, options.ChatCompletionModelId, options.ToolCompletionModelId, options.ToolReasoningModelId));

                    var client = new OpenAIClient(
                        new System.ClientModel.ApiKeyCredential(string.IsNullOrWhiteSpace(options.ApiKey) ? "ollama" : options.ApiKey),
                        CreateClientOptions(options.Endpoint));
                    return client.GetChatClient(SelectModel(purpose, options.ChatCompletionModelId, options.ToolCompletionModelId, options.ToolReasoningModelId));
                }

                if (providerOptions.Kind.Equals("AzureAIFoundry", StringComparison.OrdinalIgnoreCase))
                {
                    var options = provider.GetRequiredService<IOptions<AzureAIFoundryOptions>>().Value;
                    logger.LogInformation("Agent provider resolved to {Provider}. Endpoint: {Endpoint}. Model: {Model}",
                        providerOptions.Kind,
                        options.Endpoint,
                        SelectModel(purpose, options.ChatCompletionModelId, options.ToolCompletionModelId, options.ToolReasoningModelId));

                    var client = new OpenAIClient(
                        new System.ClientModel.ApiKeyCredential(options.ApiKey),
                        CreateClientOptions(options.Endpoint));
                    return client.GetChatClient(SelectModel(purpose, options.ChatCompletionModelId, options.ToolCompletionModelId, options.ToolReasoningModelId));
                }

                var openAIOptions = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
                logger.LogInformation("Agent provider resolved to {Provider}. Model: {Model}",
                    providerOptions.Kind,
                    SelectModel(purpose, openAIOptions.ChatCompletionModelId, openAIOptions.ToolCompletionModelId, openAIOptions.ToolReasoningModelId));

                return new ChatClient(SelectModel(purpose, openAIOptions.ChatCompletionModelId, openAIOptions.ToolCompletionModelId, openAIOptions.ToolReasoningModelId), openAIOptions.ApiKey);
            });

            services.AddKeyedSingleton<AIAgent>(purpose, (provider, _) =>
            {
                var chatClient = provider.GetRequiredKeyedService<ChatClient>(purpose);
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var governedOptions = provider.GetRequiredService<IOptions<GovernedIntentOptions>>().Value;
                var searchOptions = provider.GetRequiredService<IOptions<SearchProviderOptions>>().Value;
                var tools = new List<AITool>();
                if (purpose == InferencePurpose.Tool)
                {
                    tools.AddRange(provider.GetRequiredService<MyChatSessionsTool>().CreateFunctions());
                    tools.AddRange(provider.GetRequiredService<ActorsTool>().CreateFunctions());
                    tools.AddRange(provider.GetRequiredService<MyChatMessagesTool>().CreateFunctions());

                    if (searchOptions.IsAvailable)
                    {
                        tools.AddRange(provider.GetRequiredService<WebSearchTool>().CreateFunctions());
                    }
                }

                var agentOptions = new ChatClientAgentOptions
                {
                    Name = $"CopilotAgent{purpose}",
                    Description = "GoodToCode AgentStart Copilot",
                    ChatOptions = new Microsoft.Extensions.AI.ChatOptions
                    {
                        Tools = tools
                    }
                };

                return chatClient.AsAIAgent(
                    options: agentOptions,
                    clientFactory: inner => new ChatClientBuilder(inner)
                        .UseGovernedIntent(new GovernedChatClientOptions
                        {
                            IntentRequired = governedOptions.IntentRequired,
                            PreambleMode = governedOptions.PreambleMode
                        })
                        .Build(provider),
                    loggerFactory: loggerFactory,
                    services: provider
                );
            });
        }

        services.AddSingleton(provider => provider.GetRequiredKeyedService<ChatClient>(InferencePurpose.Chat));
        services.AddSingleton(provider => provider.GetRequiredKeyedService<AIAgent>(InferencePurpose.Tool));

        return services;
    }

    private static string SelectModel(InferencePurpose purpose, string chat, string tool, string reasoning)
        => purpose switch
        {
            InferencePurpose.Chat => chat,
            InferencePurpose.Tool => tool,
            InferencePurpose.ToolReasoning => reasoning,
            _ => throw new ArgumentOutOfRangeException(nameof(purpose))
        };

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
            if (options.IsAvailable)
            {
                return provider.GetRequiredService<BingSearchProvider>();
            }

            return provider.GetRequiredService<NoOpWebSearchProvider>();
        });

        return services;
    }

    // Chat completions that fall through to the LLM commonly take 10+ seconds; keep this below Presentation.Web's BackendApi:TimeoutSeconds so the API fails cleanly first.
    private static readonly TimeSpan ModelRequestTimeout = TimeSpan.FromSeconds(150);

    private static OpenAIClientOptions CreateClientOptions(string endpoint) => new()
    {
        Endpoint = new Uri(endpoint),
        NetworkTimeout = ModelRequestTimeout
    };

    private static bool IsSelectedProvider(AgentProviderOptions options, string providerKind)
        => options.Kind.Equals(providerKind, StringComparison.OrdinalIgnoreCase);

    private static bool HasRequiredOpenAI(OpenAIOptions options)
        => HasValue(options.ChatCompletionModelId)
           && HasValue(options.ToolCompletionModelId)
           && HasValue(options.ToolReasoningModelId)
           && HasValue(options.TextGenerationModelId)
           && HasValue(options.TextEmbeddingModelId)
           && HasValue(options.TextModerationModelId)
           && HasValue(options.ImageModelId)
           && HasValue(options.AudioModelId)
           && HasValue(options.ApiKey);

    private static bool HasRequiredGitHubCopilotSdk(GitHubCopilotSdkOptions options)
        => HasValue(options.ChatCompletionModelId)
           && HasValue(options.ToolCompletionModelId)
           && HasValue(options.ToolReasoningModelId)
           && HasValue(options.TextGenerationModelId)
           && HasValue(options.TextEmbeddingModelId)
           && HasValue(options.TextModerationModelId)
           && HasValue(options.ImageModelId)
           && HasValue(options.AudioModelId)
           && HasValue(options.ApiKey)
           && HasUri(options.Endpoint);

    private static bool HasRequiredAzureOpenAI(AzureOpenAIOptions options)
        => HasValidAnnotations(options)
           && HasValue(options.ChatDeploymentName)
           && HasValue(options.ToolDeploymentName)
           && HasValue(options.ToolReasoningDeploymentName)
           && HasValue(options.ApiKey)
           && HasUri(options.Endpoint);

    internal static string NormalizeAzureOpenAIEndpoint(string endpoint)
    {
        var trimmed = endpoint.Trim();

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return trimmed;
        }

        var path = uri.AbsolutePath.TrimEnd('/');
        if (path.Equals("/openai/v1", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/openai", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        if (path.Length == 0 || path.Equals("/", StringComparison.Ordinal))
        {
            return $"{trimmed.TrimEnd('/')}/openai/v1/";
        }

        return trimmed;
    }

    private static bool HasRequiredOllama(OllamaOptions options)
        => HasValue(options.ChatCompletionModelId)
           && HasValue(options.ToolCompletionModelId)
           && HasValue(options.ToolReasoningModelId)
           && HasUri(options.Endpoint);

    private static bool HasRequiredAzureAIFoundry(AzureAIFoundryOptions options)
        => HasValidAnnotations(options)
           && HasValue(options.ChatCompletionModelId)
           && HasValue(options.ToolCompletionModelId)
           && HasValue(options.ToolReasoningModelId)
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

    private static bool HasValue(string value)
        => !string.IsNullOrWhiteSpace(value);

    private static bool HasValidAnnotations(object options)
        => System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            options, new System.ComponentModel.DataAnnotations.ValidationContext(options), null, validateAllProperties: true);

    private static bool HasUri(string value)
        => Uri.TryCreate(value, UriKind.Absolute, out _);
}
