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

    [TestMethod]
    public async Task SemanticClassifierSearchesOnlyNonParameterizedIntents()
    {
        var store = new FakeEmbeddingStore(null);
        var classifier = new SemanticIntentClassifier(
            new IntentCatalog(
            [
                new IntentDefinition("list-actors", ["list actors"]),
                new IntentDefinition("find-actor", ["find an actor by name"],
                    [new PhraseCapture("find an actor by name ", "name", CaptureKind.Rest)])
            ]),
            new FakeEmbeddingGenerator(),
            store,
            Options.Create(new IntentClassificationOptions { EnableSemantic = true }),
            NullLogger<SemanticIntentClassifier>.Instance);

        await classifier.ClassifyAsync("show me the people", cancellationToken: CancellationToken.None);

        Assert.IsNotNull(store.EligibleIntentNames);
        CollectionAssert.Contains(store.EligibleIntentNames.ToList(), "list-actors");
        CollectionAssert.DoesNotContain(store.EligibleIntentNames.ToList(), "find-actor");
    }

    [TestMethod]
    public async Task DefaultCatalogRoutesPerSessionMessagePromptsWithSessionCapture()
    {
        var sessionId = Guid.NewGuid();
        var classifier = new RuleIntentClassifier(DefaultIntentCatalogFactory.Create());

        foreach (var prompt in new[]
        {
            $"Show messages for chat session {sessionId:D}",
            $"Can you pull up the conversation history for chat session {sessionId:D}?"
        })
        {
            var result = await classifier.ClassifyAsync(prompt);

            Assert.IsNotNull(result);
            Assert.AreEqual(IntentNames.QueryChatMessagesForSession, result!.Intent.Name);
            Assert.AreEqual(sessionId.ToString("D"), result.Captures!["sessionId"]);
        }
    }

    [TestMethod]
    public async Task DefaultCatalogRoutesMidChainMyChatSessionsEntryPoint()
    {
        var classifier = new RuleIntentClassifier(DefaultIntentCatalogFactory.Create());

        foreach (var prompt in new[]
        {
            "List my chat sessions",
            "Please list my recent chat sessions",
            "Show my conversations"
        })
        {
            var result = await classifier.ClassifyAsync(prompt);

            Assert.IsNotNull(result, $"Expected a deterministic match for '{prompt}'.");
            Assert.AreEqual(IntentNames.QueryChatSessionsList, result!.Intent.Name);
        }
    }

    [TestMethod]
    public async Task DefaultCatalogRoutesMidChainMyMessagesForCurrentChatSessionEntryPoint()
    {
        var classifier = new RuleIntentClassifier(DefaultIntentCatalogFactory.Create());

        foreach (var prompt in new[]
        {
            "List my messages for this chat session",
            "Show my messages for this chat session",
            "List messages for this chat session"
        })
        {
            var result = await classifier.ClassifyAsync(prompt);

            Assert.IsNotNull(result, $"Expected a deterministic match for '{prompt}'.");
            Assert.AreEqual(IntentNames.QueryMyChatMessagesForCurrentChatSession, result!.Intent.Name);
        }
    }

    [TestMethod]
    public async Task TokenRulesMatchReorderedActorAndChatWording()
    {
        var classifier = new RuleIntentClassifier(DefaultIntentCatalogFactory.Create());

        var actorResult = await classifier.ClassifyAsync("Could you find this actor by name of Ada Lovelace?");
        var chatResult = await classifier.ClassifyAsync("Please, for my recent chat sessions, show the history.");

        Assert.IsNotNull(actorResult);
        Assert.AreEqual(IntentNames.QueryActorsByName, actorResult!.Intent.Name);
        Assert.AreEqual("Ada Lovelace?", actorResult.Captures!["name"]);
        Assert.IsNotNull(chatResult);
        Assert.AreEqual(IntentNames.QueryChatSessionsList, chatResult!.Intent.Name);
    }

    [TestMethod]
    public async Task TokenRulesPreferMoreSpecificIntentAndRejectBlockers()
    {
        var classifier = new RuleIntentClassifier(new IntentCatalog(
        [
            new IntentDefinition("broad", [], TokenRule: new IntentTokenRule(["actor"], [["show"]])),
            new IntentDefinition("specific", [], TokenRule: new IntentTokenRule(["actor", "my"], [["show"]]))
        ]));

        var specific = await classifier.ClassifyAsync("show my actors");
        var blocked = await new RuleIntentClassifier(new IntentCatalog(
        [
            new IntentDefinition("messages", [], TokenRule: new IntentTokenRule(["actor"], [["show"]], ["message"]))
        ])).ClassifyAsync("show actor messages");

        Assert.IsNotNull(specific);
        Assert.AreEqual("specific", specific!.Intent.Name);
        Assert.IsNull(blocked);
    }

    [TestMethod]
    public async Task AmbiguousEqualSpecificityTokenRulesFallThrough()
    {
        var classifier = new RuleIntentClassifier(new IntentCatalog(
        [
            new IntentDefinition("first", [], TokenRule: new IntentTokenRule(["actor"], [["find"]])),
            new IntentDefinition("second", [], TokenRule: new IntentTokenRule(["actor"], [["find"]]))
        ]));

        var result = await classifier.ClassifyAsync("find actor");

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
        public IReadOnlySet<string>? EligibleIntentNames { get; private set; }

        public Task UpsertIntentEmbeddingsAsync(string intentName, IEnumerable<Embedding> embeddings, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<EmbeddingMatch>> SearchAsync(float[] queryVector, CancellationToken cancellationToken, int topK = 5, float similarityThreshold = 0.75f, IReadOnlySet<string>? eligibleIntentNames = null)
        {
            EligibleIntentNames = eligibleIntentNames;
            return Task.FromResult<IReadOnlyList<EmbeddingMatch>>(match is null ? [] : [match]);
        }

        public Task DeleteIntentEmbeddingsAsync(string intentName, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> IsReadyAsync(CancellationToken cancellationToken) => Task.FromResult(true);
    }
}
