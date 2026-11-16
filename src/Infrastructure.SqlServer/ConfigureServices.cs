using Goodtocode.AgentStart.Core.Application.Abstractions;
using Goodtocode.AgentStart.Core.Application.Playbooks.SqlStatistics;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Embeddings;
using Goodtocode.AgentStart.Infrastructure.SqlServer.Persistence;
using Goodtocode.AgentStart.Infrastructure.SqlServer.Playbooks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.AgentStart.Infrastructure.SqlServer;

public static class ConfigureServices
{
    public static IServiceCollection AddDbContextServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                .UseLazyLoadingProxies());

        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        services.AddHttpClient<AzureOpenAiEmbeddingGenerator>();
        services.AddScoped<IEmbeddingGenerator>(provider =>
            provider.GetRequiredService<AzureOpenAiEmbeddingGenerator>());
        services.AddScoped<IIntentEmbeddingStore, Embeddings.SqlIntentEmbeddingStore>();
        services.AddScoped<ISqlDatabaseStatisticsProvider, SqlServerDatabaseStatisticsProvider>();

        return services;
    }
}