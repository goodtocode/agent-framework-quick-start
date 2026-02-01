using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Plugins;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using Microsoft.SemanticKernel.TextToAudio;
using Microsoft.SemanticKernel.TextToImage;

namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework;

public static class ConfigureServices
{
    public static IServiceCollection AddAgentFrameworkMemoryServices(this IServiceCollection services)
    {
        //var memory = new KernelMemoryBuilder()
        //    .WithOpenAIDefaults(Env.Var("OPENAI_API_KEY"))
        //    .WithSqlServerMemoryDb("YourSqlConnectionString")
        //    .Build<MemoryServerless>();

        return services;
    }

    public static IServiceCollection AddAgentFrameworkOpenAIServices(this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddOptions<OpenAIOptions>()
        .Bind(configuration.GetSection(OpenAIOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Plugins
        services.AddSingleton<IActorsPlugin, ActorsPlugin>();
        services.AddSingleton<IChatSessionsPlugin, ChatSessionsPlugin>();
        services.AddSingleton<IChatMessagesPlugin, ChatMessagesPlugin>();

        // TextGenerationService deprecated. Use custom connector service instead.
        services.AddSingleton<ITextGenerationService, TextGenerationService>();

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        // Translate audio to text
        services.AddSingleton<IAudioToTextService>(sp =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            return kernel.GetRequiredService<IAudioToTextService>();
        })
        // Translate audio to text
        .AddSingleton<ITextToAudioService>(sp =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            return kernel.GetRequiredService<ITextToAudioService>();
        })
        // Translate text to image
        .AddSingleton<ITextToImageService>(sp =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            return kernel.GetRequiredService<ITextToImageService>();
        });
#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0010

        // Chat Completion
        services.AddSingleton<IChatCompletionService>(sp =>
        { 
            var kernel = sp.GetRequiredService<Kernel>();
            return kernel.GetRequiredService<IChatCompletionService>();
        });

        // To Register the Kernel with no plugins: services.AddKernel();
        // Register the Kernel with plugins imported: 
        services.AddSingleton<Kernel>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            var builder = Kernel.CreateBuilder();

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            // AI Services
            builder.Services
                .AddOpenAIChatCompletion(modelId: options.ChatCompletionModelId, apiKey: options.ApiKey)
                .AddOpenAIAudioToText(modelId: options.AudioModelId, apiKey: options.ApiKey)
                .AddOpenAITextToAudio(modelId: options.AudioModelId, apiKey: options.ApiKey)                
                .AddOpenAITextToImage(modelId: options.ImageModelId, apiKey: options.ApiKey);
#pragma warning restore SKEXP0010

            // Logging  
            builder.Services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
            });

            // Memory - ToDo: .WithMemoryStore(new VolatileMemoryStore());

            var kernel = builder.Build();

            var authorsPlugin = sp.GetRequiredService<IActorsPlugin>();
            var chatSessionsPlugin = sp.GetRequiredService<IChatSessionsPlugin>();
            var chatMessagesPlugin = sp.GetRequiredService<IChatMessagesPlugin>();

            kernel.ImportPluginFromObject(authorsPlugin, nameof(ActorsPlugin));
            kernel.ImportPluginFromObject(chatSessionsPlugin, nameof(ChatSessionsPlugin));
            kernel.ImportPluginFromObject(chatMessagesPlugin, nameof(ChatMessagesPlugin));

            return kernel;
        });

        return services;
    }
}