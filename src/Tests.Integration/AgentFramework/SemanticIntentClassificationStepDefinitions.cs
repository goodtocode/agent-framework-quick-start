using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Embeddings;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;
using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Options;
using Goodtocode.AgentFramework.Infrastructure.SqlServer.Embeddings;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Goodtocode.AgentFramework.Tests.Integration.AgentFramework;

[Binding]
[Scope(Tag = "semanticIntentClassification")]
public sealed class SemanticIntentClassificationStepDefinitions : TestBase
{
    private bool _enabled;
    private IntentCatalog _catalog = new([]);
    private HybridIntentClassifier? _classifier;
    private IntentMatch? _result;

    [Given("the semantic classifier switch is \"(.*)\"")]
    public void GivenTheSemanticClassifierSwitchIs(string enabled)
    {
        _enabled = bool.Parse(enabled);
    }

    [Given("the catalog contains intent \"(.*)\" with example \"(.*)\"")]
    public void GivenTheCatalogContainsIntentWithExample(string intentName, string example)
    {
        _catalog = new IntentCatalog([new IntentDefinition(intentName, [example])]);
    }

    [Given("the semantic example \"(.*)\" is seeded for intent \"(.*)\"")]
    public async Task GivenTheSemanticExampleIsSeededForIntent(string example, string intentName)
    {
        var store = new SqlIntentEmbeddingStore(context, NullLogger<SqlIntentEmbeddingStore>.Instance);
        await store.UpsertIntentEmbeddingsAsync(intentName, [new Embedding
        {
            Id = Guid.NewGuid(),
            IntentName = intentName,
            Source = EmbeddingSource.Example,
            SourceText = example,
            Vector = [1f, 0f],
            Weight = 1f,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        }], CancellationToken.None);

        _classifier = new HybridIntentClassifier(
            new RuleIntentClassifier(_catalog),
            new SemanticIntentClassifier(
                _catalog,
                new FixedEmbeddingGenerator(),
                store,
                Options.Create(new IntentClassificationOptions
                {
                    EnableSemantic = _enabled,
                    SemanticThreshold = 0.75f,
                    TopKResults = 5
                }),
                NullLogger<SemanticIntentClassifier>.Instance),
            Options.Create(new IntentClassificationOptions { EnableSemantic = _enabled }),
            NullLogger<HybridIntentClassifier>.Instance);
    }

    [When("I classify the message \"(.*)\"")]
    public async Task WhenIClassifyTheMessage(string message)
    {
        _result = await _classifier!.ClassifyAsync(message, cancellationToken: CancellationToken.None);
    }

    [Then("the classified intent is \"(.*)\"")]
    public void ThenTheClassifiedIntentIs(string expected)
    {
        if (expected.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            Assert.IsNull(_result);
            return;
        }

        Assert.IsNotNull(_result);
        Assert.AreEqual(expected, _result!.Intent.Name);
    }

    private sealed class FixedEmbeddingGenerator : IEmbeddingGenerator
    {
        public int Dimension => 2;
        public Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken) => Task.FromResult(new[] { 1f, 0f });
        public Task<IDictionary<string, float[]>> GenerateBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken) =>
            Task.FromResult<IDictionary<string, float[]>>(texts.ToDictionary(text => text, _ => new[] { 1f, 0f }));
    }
}
