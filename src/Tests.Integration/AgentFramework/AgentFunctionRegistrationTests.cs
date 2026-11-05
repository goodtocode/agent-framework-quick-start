using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.Reflection;
using System.Text.Json;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Tools;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentFramework.Tests.Integration.AgentFramework;

[TestClass]
public sealed class AgentFunctionRegistrationTests
{
    [TestMethod]
    public void EveryDescribedToolMethodHasAFunctionSchema()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        ScopedAgentTool[] tools =
        [
            new MyChatSessionsTool(provider),
            new ActorsTool(provider, new AgentChatContextAccessor()),
            new MyChatMessagesTool(provider),
            new WebSearchTool(provider)
        ];
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var tool in tools)
        {
            var methods = tool.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.GetCustomAttribute<DescriptionAttribute>() is not null).ToList();
            var functions = tool.CreateFunctions();
            Assert.IsNotEmpty(functions);
            Assert.AreEqual(methods.Count, functions.Count);
            foreach (var method in methods)
            {
                var function = functions.Single(candidate => candidate.Name == $"{tool.GetType().Name}_{method.Name}");
                Assert.IsTrue(names.Add(function.Name));
                Assert.AreEqual(method.GetCustomAttribute<DescriptionAttribute>()!.Description, function.Description);
                var properties = function.JsonSchema.GetProperty("properties");
                Assert.IsFalse(properties.TryGetProperty("cancellationToken", out _));
                foreach (var parameter in method.GetParameters().Where(parameter => parameter.ParameterType != typeof(CancellationToken)))
                {
                    Assert.IsTrue(properties.TryGetProperty(parameter.Name!, out _));
                }
            }
        }
    }

    [TestMethod]
    public async Task FunctionInvocationPreservesScopedResolution()
    {
        var state = new InvocationState();
        var services = new ServiceCollection();
        services.AddSingleton(state);
        services.AddScoped<YearProvider>();
        using var provider = services.BuildServiceProvider(validateScopes: true);
        var function = new YearTool(provider).CreateFunctions().Single();

        var result = await function.InvokeAsync(new AIFunctionArguments(), CancellationToken.None);

        Assert.IsTrue(result is JsonElement { ValueKind: JsonValueKind.Number });
        if (result is JsonElement element)
        {
            Assert.AreEqual(DateTime.UtcNow.Year, element.GetInt32());
        }
        Assert.AreEqual(1, state.Invocations);
        Assert.AreEqual(1, state.Disposals);
    }

    [TestMethod]
    public void ToolWithoutDescribedMethodsFailsExplicitly()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        Assert.ThrowsExactly<InvalidOperationException>(() => new EmptyTool(provider).CreateFunctions());
    }

    private sealed class EmptyTool(IServiceProvider provider) : ScopedAgentTool(provider);

    private sealed class YearTool(IServiceProvider provider) : ScopedAgentTool(provider)
    {
        [Description("Returns the current UTC year using a scoped service.")]
        public Task<int> GetYearAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ResolveScopedAsync(scoped => Task.FromResult(scoped.GetRequiredService<YearProvider>().GetYear()));
        }
    }

    private sealed class InvocationState
    {
        public int Invocations { get; set; }
        public int Disposals { get; set; }
    }

    private sealed class YearProvider(InvocationState state) : IDisposable
    {
        public int GetYear()
        {
            state.Invocations++;
            return DateTime.UtcNow.Year;
        }

        public void Dispose() => state.Disposals++;
    }
}
