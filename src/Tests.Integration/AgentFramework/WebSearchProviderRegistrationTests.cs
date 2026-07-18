using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Providers;

namespace Goodtocode.AgentFramework.Tests.Integration.AgentFramework;

[TestClass]
public sealed class WebSearchProviderRegistrationTests
{
    [TestMethod]
    public void AddWebSearchProvidersUsesNoOpProviderWhenBingConfigurationIsMissing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SearchProvider:Provider"] = "Bing",
                ["SearchProvider:Endpoint"] = "https://api.bing.microsoft.com",
                ["SearchProvider:ApiKey"] = string.Empty,
                ["SearchProvider:DefaultResultCount"] = "5"
            })
            .Build();

        services.AddLogging();
        services.AddWebSearchProviders(configuration);

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var provider = scope.ServiceProvider.GetRequiredService<IWebSearchProvider>();

        Assert.IsInstanceOfType<NoOpWebSearchProvider>(provider);
    }
}
