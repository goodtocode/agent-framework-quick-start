using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Goodtocode.AgentStart.Infrastructure.AgentFramework;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Governance;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Options;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Playbooks.Essay;
using Goodtocode.AgentStart.Infrastructure.AgentFramework.Playbooks.Taxonomy;
using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace Goodtocode.AgentStart.Tests.Integration.AgentFramework;

[TestClass]
public sealed class InferencePurposeTests
{
    [TestMethod]
    public void ClientsResolveTheirExplicitPurposeDeployments()
    {
        using var provider = CreateServices().BuildServiceProvider();

        foreach (var (purpose, model) in new[]
        {
            (InferencePurpose.Chat, "chat-fast"),
            (InferencePurpose.Tool, "tool-standard"),
            (InferencePurpose.ToolReasoning, "tool-reasoning")
        })
        {
            var client = provider.GetRequiredKeyedService<ChatClient>(purpose).AsIChatClient();
            Assert.AreEqual(model, client.GetService<ChatClientMetadata>()?.DefaultModelId);
        }
        Assert.AreSame(provider.GetRequiredKeyedService<AIAgent>(InferencePurpose.Tool), provider.GetRequiredService<AIAgent>());
    }

    [TestMethod]
    [DataRow("AzureOpenAI:ToolDeploymentName", "")]
    [DataRow("AzureOpenAI:ToolDeploymentName", "chat-fast")]
    [DataRow("AzureOpenAI:ToolReasoningDeploymentName", "chat-fast")]
    [DataRow("AzureOpenAI:ChatDeploymentName", "tool-standard")]
    [DataRow("AzureOpenAI:EmbeddingDeploymentName", "tool-standard")]
    public void MissingOrWrongPurposeConfigurationFailsValidation(string key, string value)
    {
        using var provider = CreateServices(key, value).BuildServiceProvider();
        Assert.ThrowsExactly<OptionsValidationException>(() => provider.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value);
    }

    [TestMethod]
    public void PlaybookInferenceUsesReasoningAgent()
    {
        Type[] tools =
        [
            typeof(TaxonomyCollectAgentTool),
            typeof(TaxonomyEvaluateAgentTool),
            typeof(TaxonomyRecordAgentTool),
            typeof(EssayEvaluateAgentTool)
        ];
        foreach (var tool in tools)
        {
            var agentParameter = tool.GetConstructors().Single().GetParameters().Single(parameter => parameter.ParameterType == typeof(AIAgent));
            Assert.AreEqual(InferencePurpose.ToolReasoning, agentParameter.GetCustomAttribute<FromKeyedServicesAttribute>()?.Key);
        }
    }

    [TestMethod]
    [DataRow(InferencePurpose.Chat, false)]
    [DataRow(InferencePurpose.Tool, true)]
    [DataRow(InferencePurpose.ToolReasoning, false)]
    public async Task OnlyInteractiveAgentAdvertisesApplicationFunctions(InferencePurpose purpose, bool expectsTools)
    {
        var handler = new RecordingHandler();
        using var http = new HttpClient(handler);
        var services = CreateServices();
        services.AddKeyedSingleton<ChatClient>(purpose, (_, _) =>
            new OpenAIClient(new ApiKeyCredential("synthetic-test-key"), new OpenAIClientOptions
            {
                Endpoint = new Uri("https://example.test/openai/v1/"),
                Transport = new HttpClientPipelineTransport(http)
            }).GetChatClient("synthetic-deployment"));
        using var provider = services.BuildServiceProvider();
        using var governed = provider.GetRequiredService<ChatGovernanceScope>().Begin(
            Guid.NewGuid(), AgentInstruction.Create("Reply with OK.", InstructionScope.Session));

        await provider.GetRequiredKeyedService<AIAgent>(purpose).RunAsync("Reply with OK.", cancellationToken: CancellationToken.None);

        Assert.AreEqual(expectsTools, handler.FunctionNames.Count > 0);
        if (expectsTools)
        {
            Assert.IsTrue(handler.FunctionNames.All(name => name.Length <= 64));
            CollectionAssert.Contains(handler.FunctionNames, "MyChatSessionsTool_ListRecentSessionsAsync");
            CollectionAssert.Contains(handler.FunctionNames, "MyChatMessagesTool_ListRecentMessagesAsync");
            Assert.AreEqual(handler.FunctionNames.Count, handler.FunctionNames.Distinct().Count());
        }
    }

    private static IServiceCollection CreateServices(string? key = null, string? value = null)
    {
        var values = new Dictionary<string, string?>
        {
            ["AgentProvider:Kind"] = "AzureOpenAI",
            ["AzureOpenAI:ChatDeploymentName"] = "chat-fast",
            ["AzureOpenAI:ToolDeploymentName"] = "tool-standard",
            ["AzureOpenAI:ToolReasoningDeploymentName"] = "tool-reasoning",
            ["AzureOpenAI:ApiKey"] = "synthetic-test-key",
            ["AzureOpenAI:Endpoint"] = "https://example.test/openai/v1/",
            ["SearchProvider:Provider"] = "None"
        };
        if (key is not null)
        {
            values[key] = value;
        }
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAgentFrameworkOpenAIServices(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
        return services;
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<string> FunctionNames { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            if (body.RootElement.TryGetProperty("tools", out var tools))
            {
                foreach (var tool in tools.EnumerateArray())
                {
                    FunctionNames.Add(tool.GetProperty("function").GetProperty("name").GetString()!);
                }
            }
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"id":"synthetic-completion","object":"chat.completion","created":0,"model":"synthetic-model","choices":[{"index":0,"message":{"role":"assistant","content":"OK"},"finish_reason":"stop"}],"usage":{"prompt_tokens":1,"completion_tokens":1,"total_tokens":2}}""",
                    Encoding.UTF8, "application/json")
            };
        }
    }
}
