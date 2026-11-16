using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.Reflection;
using System.Text.Json;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Tools;
using Microsoft.Extensions.AI;

namespace Goodtocode.AgentStart.Tests.Integration.AgentFramework;

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
                Assert.IsTrue(function.Name.Length <= 64);
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

    [TestMethod]
    public async Task LongFunctionNamesAreStableDistinctAndInvocable()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var tool = new LongNameTool(provider);
        var functions = tool.CreateFunctions();
        var repeated = tool.CreateFunctions();

        Assert.AreEqual(2, functions.Count);
        Assert.AreEqual(2, functions.Select(function => function.Name).Distinct().Count());
        CollectionAssert.AreEqual(functions.Select(function => function.Name).ToArray(), repeated.Select(function => function.Name).ToArray());
        foreach (var function in functions)
        {
            Assert.AreEqual(64, function.Name.Length);
            StringAssert.Matches(function.Name, new System.Text.RegularExpressions.Regex("^[a-zA-Z0-9_-]{64}$"));
            var result = await function.InvokeAsync(new AIFunctionArguments(), CancellationToken.None);
            Assert.IsInstanceOfType<JsonElement>(result);
            if (result is JsonElement element)
            {
                Assert.AreEqual(1, element.GetInt32());
            }
        }
    }

    private sealed class LongNameTool(IServiceProvider provider) : ScopedAgentTool(provider)
    {
        [Description("Synthetic function with a long name.")]
        public int QuerySyntheticFunctionWithAnExtremelyLongSharedNameForProviderBoundaryFirst() => 1;

        [Description("Another synthetic function with the same long prefix.")]
        public int QuerySyntheticFunctionWithAnExtremelyLongSharedNameForProviderBoundarySecond() => 1;
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
