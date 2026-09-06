using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Embeddings;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Goodtocode.AgentFramework.Tests.Integration.AgentFramework;

[TestClass]
public sealed class IntentClassifierTests
{
    [TestMethod]
    public async Task SemanticClassifierReturnsCatalogIntentForMatchingEmbedding()
    {
        var classifier = CreateSemanticClassifier(new EmbeddingMatch
        {
            IntentName = "list-actors",
            Source = EmbeddingSource.Example,
            SourceText = "show actors",
            SimilarityScore = 0.9f
        });

        var result = await classifier.ClassifyAsync("could you show me the people", cancellationToken: CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual("list-actors", result.Intent.Name);
    }

    [TestMethod]
    public async Task HybridClassifierUsesRuleMatchBeforeSemanticFallback()
    {
        var catalog = new IntentCatalog([new IntentDefinition("list-actors", ["show actors"])]);
        var rule = new RuleIntentClassifier(catalog);
        var semantic = CreateSemanticClassifier(null);
        var hybrid = new HybridIntentClassifier(
            rule,
            semantic,
            Options.Create(new IntentClassificationOptions { EnableSemantic = true }),
            NullLogger<HybridIntentClassifier>.Instance);

        var result = await hybrid.ClassifyAsync("show actors", cancellationToken: CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual("list-actors", result.Intent.Name);
    }

    [TestMethod]
    public async Task HybridClassifierSkipsSemanticWhenDisabled()
    {
        var catalog = new IntentCatalog([new IntentDefinition("list-actors", ["show actors"])]);
        var hybrid = new HybridIntentClassifier(
            new RuleIntentClassifier(catalog),
            CreateSemanticClassifier(new EmbeddingMatch
            {
                IntentName = "list-actors",
                Source = EmbeddingSource.Example,
                SourceText = "show actors",
                SimilarityScore = 0.9f
            }),
            Options.Create(new IntentClassificationOptions { EnableSemantic = false }),
            NullLogger<HybridIntentClassifier>.Instance);

        var result = await hybrid.ClassifyAsync("unmatched wording", cancellationToken: CancellationToken.None);

        Assert.IsNull(result);
    }

    private static SemanticIntentClassifier CreateSemanticClassifier(EmbeddingMatch? match)
    {
        var catalog = new IntentCatalog([new IntentDefinition("list-actors", ["show actors"])]);
        return new SemanticIntentClassifier(
            catalog,
            new FakeEmbeddingGenerator(),
            new FakeEmbeddingStore(match),
            Options.Create(new IntentClassificationOptions { EnableSemantic = true }),
            NullLogger<SemanticIntentClassifier>.Instance);
    }

    private sealed class FakeEmbeddingGenerator : IEmbeddingGenerator
    {
        public int Dimension => 2;
        public Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken) => Task.FromResult(new[] { 1f, 0f });
        public Task<IDictionary<string, float[]>> GenerateBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken) =>
            Task.FromResult<IDictionary<string, float[]>>(texts.ToDictionary(text => text, _ => new[] { 1f, 0f }));
    }

    private sealed class FakeEmbeddingStore(EmbeddingMatch? match) : IIntentEmbeddingStore
    {
        public Task UpsertIntentEmbeddingsAsync(string intentName, IEnumerable<Embedding> embeddings, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<EmbeddingMatch>> SearchAsync(float[] queryVector, CancellationToken cancellationToken, int topK = 5, float similarityThreshold = 0.75f) =>
            Task.FromResult<IReadOnlyList<EmbeddingMatch>>(match is null ? [] : [match]);
        public Task DeleteIntentEmbeddingsAsync(string intentName, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> IsReadyAsync(CancellationToken cancellationToken) => Task.FromResult(true);
    }
}
