using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Embeddings;
using Goodtocode.AgentFramework.Infrastructure.SqlServer.Embeddings;
using Goodtocode.AgentFramework.Infrastructure.SqlServer.Persistence;
using Microsoft.Extensions.Logging.Abstractions;

namespace Goodtocode.AgentFramework.Tests.Integration.Infrastructure;

[TestClass]
public sealed class SqlIntentEmbeddingStoreTests
{
    [TestMethod]
    public async Task UpsertStoresAndReplacesEmbeddings()
    {
        await using var context = CreateContext();
        var store = CreateStore(context);

        await store.UpsertIntentEmbeddingsAsync("CreatePlaybook", CreateEmbeddings(), CancellationToken.None);
        await store.UpsertIntentEmbeddingsAsync("CreatePlaybook", CreateEmbeddings().Take(1), CancellationToken.None);

        Assert.AreEqual(1, await context.IntentEmbeddings.CountAsync());
    }

    [TestMethod]
    public async Task SearchReturnsWeightedTopMatch()
    {
        await using var context = CreateContext();
        var store = CreateStore(context);
        await store.UpsertIntentEmbeddingsAsync("CreatePlaybook", CreateEmbeddings(), CancellationToken.None);

        var result = await store.SearchAsync(new[] { 1f, 0f }, CancellationToken.None, topK: 1, similarityThreshold: 0.5f);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("CreatePlaybook", result[0].IntentName);
        Assert.AreEqual(1f, result[0].SimilarityScore, 0.001f);
    }

    [TestMethod]
    public async Task DeleteRemovesEmbeddingsAndReadinessReflectsStore()
    {
        await using var context = CreateContext();
        var store = CreateStore(context);
        await store.UpsertIntentEmbeddingsAsync("CreatePlaybook", CreateEmbeddings().Take(1), CancellationToken.None);

        Assert.IsTrue(await store.IsReadyAsync(CancellationToken.None));
        await store.DeleteIntentEmbeddingsAsync("CreatePlaybook", CancellationToken.None);

        Assert.IsFalse(await store.IsReadyAsync(CancellationToken.None));
    }

    private static SqlIntentEmbeddingStore CreateStore(AgentFrameworkContext context)
        => new(context, NullLogger<SqlIntentEmbeddingStore>.Instance);

    private static AgentFrameworkContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AgentFrameworkContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AgentFrameworkContext(options, new TestRlsContext());
    }

    private static IEnumerable<Embedding> CreateEmbeddings()
    {
        yield return new Embedding
        {
            Id = Guid.NewGuid(), IntentName = "CreatePlaybook", Source = EmbeddingSource.Example,
            SourceText = "create a playbook", Vector = new[] { 1f, 0f }, Weight = 1f
        };
        yield return new Embedding
        {
            Id = Guid.NewGuid(), IntentName = "CreatePlaybook", Source = EmbeddingSource.Example,
            SourceText = "make a playbook", Vector = new[] { 0f, 1f }, Weight = 1f
        };
    }

    private sealed class TestRlsContext : IRlsContext
    {
        public Guid OwnerId { get; } = Guid.NewGuid();
        public Guid TenantId { get; } = Guid.NewGuid();
    }
}
