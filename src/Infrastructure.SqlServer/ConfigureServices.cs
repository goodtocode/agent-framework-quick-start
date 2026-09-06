using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Embeddings;
using Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer;

public static class ConfigureServices
{
    public static IServiceCollection AddDbContextServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AgentFrameworkContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    builder => builder.MigrationsAssembly(typeof(AgentFrameworkContext).Assembly.FullName))
                .UseLazyLoadingProxies());

        services.AddScoped<IAgentFrameworkContext, AgentFrameworkContext>();
        services.AddHttpClient<AzureOpenAiEmbeddingGenerator>();
        services.AddScoped<IEmbeddingGenerator>(provider =>
            provider.GetRequiredService<AzureOpenAiEmbeddingGenerator>());
        services.AddScoped<IIntentEmbeddingStore, Embeddings.SqlIntentEmbeddingStore>();

        return services;
    }
}