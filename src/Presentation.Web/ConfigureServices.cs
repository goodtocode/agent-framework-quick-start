using System.Reflection;
using Goodtocode.AgentFramework.Presentation.Web.Infrastructure.Auth;
using Goodtocode.AgentFramework.Presentation.Web.Infrastructure.Storage;
using Goodtocode.AgentFramework.Presentation.Web.Features.Chat.Services;
using Microsoft.Extensions.Options;
using Microsoft.FluentUI.AspNetCore.Components;
using Goodtocode.AgentFramework.Presentation.Web.Library.Auth.Services;
using Goodtocode.AgentFramework.Presentation.Web.Library.Auth;
using Goodtocode.AgentFramework.Presentation.Web.Infrastructure.Options;

namespace Goodtocode.AgentFramework.Presentation.Web;

public static class ConfigureServices
{
    public static bool IsLocal(this IWebHostEnvironment environment)
    {
        return environment.EnvironmentName.Equals("Local", StringComparison.OrdinalIgnoreCase);
    }

    public static void AddLocalEnvironment(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsLocal())
        {
            builder.Configuration
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddEnvironmentVariables();
            builder.WebHost.UseStaticWebAssets();
        }
    }

    public static void AddFrontendServices(this IServiceCollection services)
    {
        services.AddScoped<IDialogService, DialogService>();
        services.AddScoped<ILocalStorageService, LocalStorageService>();
        services.AddScoped<IChatService, ChatService>();
    }

    public static IServiceCollection AddUserClaimsSyncService(this IServiceCollection services)
    {
        services.AddScoped<IClaimsReader, HttpClaimsReader>();
        services.AddScoped<IUserSyncService, UserSyncService>();

        return services;
    }

    public static IServiceCollection AddBackendApi(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<BackendApiOptions>()
        .Bind(configuration.GetSection(BackendApiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var backendApiOptions = configuration
            .GetSection(BackendApiOptions.SectionName)
            .Get<BackendApiOptions>() ?? throw new InvalidOperationException($"Missing '{BackendApiOptions.SectionName}' configuration section.");

        services.AddAccessTokenHttpClient(options =>
        {
            options.BaseAddress = backendApiOptions.BaseUrl;
            options.ClientName = "BackendApiClient";
            options.MaxRetry = 3;
        });

        services.AddScoped(provider =>
        {
            var options = provider.GetRequiredService<IOptions<BackendApiOptions>>().Value;
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("BackendApiClient");
            return new BackendApiClient(options.BaseUrl.ToString(), httpClient);
        });

        return services;
    }
}