using System.Net;
using System.Text;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Embeddings;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;

namespace Goodtocode.AgentFramework.Tests.Integration.AgentFramework;

[TestClass]
public sealed class AzureOpenAiEmbeddingGeneratorTests
{
    [TestMethod]
    public void DimensionReturns1536()
    {
        var generator = CreateGenerator(new QueueHandler());

        Assert.AreEqual(1536, generator.Dimension);
    }

    [TestMethod]
    public async Task GenerateBatchAsyncReturnsEmptyForEmptyInput()
    {
        var generator = CreateGenerator(new QueueHandler());

        var result = await generator.GenerateBatchAsync(Array.Empty<string>(), CancellationToken.None);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GenerateAsyncReturnsVectorFromAzureResponse()
    {
        var generator = CreateGenerator(new QueueHandler(CreateResponse("[0.1, 0.2, 0.3]", 0)), dimension: 3);

        var result = await generator.GenerateAsync("create a playbook", CancellationToken.None);

        CollectionAssert.AreEqual(new[] { 0.1f, 0.2f, 0.3f }, result);
    }

    [TestMethod]
    public async Task GenerateBatchAsyncMapsResponsesByInputIndex()
    {
        var handler = new QueueHandler(CreateBatchResponse());
        var generator = CreateGenerator(handler, dimension: 1);

        var result = await generator.GenerateBatchAsync(new[] { "first", "second" }, CancellationToken.None);

        CollectionAssert.AreEqual(new[] { 0.1f }, result["first"]);
        CollectionAssert.AreEqual(new[] { 0.2f }, result["second"]);
        Assert.AreEqual(1, handler.SendCount);
    }

    [TestMethod]
    public async Task GenerateAsyncUsesV1EndpointDeploymentAndDimension()
    {
        var handler = new QueueHandler(CreateResponse("[0.1, 0.2, 0.3]", 0));
        var generator = CreateGenerator(handler, dimension: 3, endpoint: "https://example.test/openai/v1/");

        await generator.GenerateAsync("synthetic intent", CancellationToken.None);

        Assert.AreEqual("https://example.test/openai/v1/embeddings", handler.RequestUri!.ToString());
        using var request = System.Text.Json.JsonDocument.Parse(handler.RequestBody!);
        Assert.AreEqual("embedding-standard", request.RootElement.GetProperty("model").GetString());
        Assert.AreEqual(3, request.RootElement.GetProperty("dimensions").GetInt32());
    }

    [TestMethod]
    public async Task GenerateAsyncRejectsUnexpectedVectorDimension()
    {
        var generator = CreateGenerator(new QueueHandler(CreateResponse("[0.1]", 0)));

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
            generator.GenerateAsync("synthetic intent", CancellationToken.None));
    }

    private static AzureOpenAiEmbeddingGenerator CreateGenerator(HttpMessageHandler handler, int dimension = 1536, string endpoint = "https://example.test")
    {
        return new AzureOpenAiEmbeddingGenerator(
            Options.Create(new AzureOpenAIOptions
            {
                ApiKey = "test-key",
                Endpoint = endpoint,
                EmbeddingDeploymentName = "embedding-standard",
                EmbeddingDimension = dimension
            }),
            NullLogger<AzureOpenAiEmbeddingGenerator>.Instance,
            new HttpClient(handler));
    }

    private static HttpResponseMessage CreateResponse(string embedding, int index)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                $"{{\"data\":[{{\"index\":{index},\"embedding\":{embedding}}}]}}",
                Encoding.UTF8,
                "application/json")
        };
    }

    private static HttpResponseMessage CreateBatchResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"data\":[{\"index\":0,\"embedding\":[0.1]},{\"index\":1,\"embedding\":[0.2]}]}",
                Encoding.UTF8,
                "application/json")
        };
    }

    private sealed class QueueHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> responses = new(responses);

        public int SendCount { get; private set; }
        public Uri? RequestUri { get; private set; }
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            RequestUri = request.RequestUri;
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return responses.Dequeue();
        }
    }
}
