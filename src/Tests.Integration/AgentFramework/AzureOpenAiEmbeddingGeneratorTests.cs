using System.Net;
using System.Text;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Embeddings;
using Microsoft.Extensions.Configuration;
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
        var generator = CreateGenerator(new QueueHandler(CreateResponse("[0.1, 0.2, 0.3]", 0)));

        var result = await generator.GenerateAsync("create a playbook", CancellationToken.None);

        CollectionAssert.AreEqual(new[] { 0.1f, 0.2f, 0.3f }, result);
    }

    [TestMethod]
    public async Task GenerateBatchAsyncMapsResponsesByInputIndex()
    {
        var handler = new QueueHandler(CreateBatchResponse());
        var generator = CreateGenerator(handler);

        var result = await generator.GenerateBatchAsync(new[] { "first", "second" }, CancellationToken.None);

        CollectionAssert.AreEqual(new[] { 0.1f }, result["first"]);
        CollectionAssert.AreEqual(new[] { 0.2f }, result["second"]);
        Assert.AreEqual(1, handler.SendCount);
    }

    private static AzureOpenAiEmbeddingGenerator CreateGenerator(HttpMessageHandler handler)
    {
        return new AzureOpenAiEmbeddingGenerator(
            Options.Create(new AzureOpenAIOptions
            {
                ApiKey = "test-key",
                Endpoint = "https://example.test",
                EmbeddingDeploymentName = "embedding-fast"
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

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            return Task.FromResult(responses.Dequeue());
        }
    }
}
