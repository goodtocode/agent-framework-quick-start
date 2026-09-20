using Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

namespace Goodtocode.AgentFramework.Tests.Integration.AgentFramework;

[Binding]
[Scope(Tag = "tokenIntentClassification")]
public sealed class TokenIntentClassificationStepDefinitions : TestBase
{
    private readonly List<IntentDefinition> _intents = [];
    private IntentMatch? _result;

    [Given("the token catalog contains intent \"(.*)\" requiring all tokens \"(.*)\" and any-of groups \"(.*)\" with blockers \"(.*)\"")]
    public void GivenTheTokenCatalogContainsIntent(
        string intent,
        string allOf,
        string anyOfGroups,
        string noneOf)
    {
        _intents.Add(CreateTokenIntent(intent, allOf, anyOfGroups, noneOf));
    }

    [Given("the token catalog contains intent \"(.*)\" requiring all tokens \"(.*)\" and any-of groups \"(.*)\" with capture prefix \"(.*)\"")]
    public void GivenTheTokenCatalogContainsIntentWithCapture(
        string intent,
        string allOf,
        string anyOfGroups,
        string prefix)
    {
        _intents.Add(CreateTokenIntent(
            intent,
            allOf,
            anyOfGroups,
            captures: [new PhraseCapture(prefix, "name", CaptureKind.Rest)]));
    }

    [Given("the token catalog contains a broad intent \"(.*)\" with all tokens \"(.*)\" and any-of groups \"(.*)\"")]
    public void GivenTheTokenCatalogContainsBroadIntent(string intent, string allOf, string anyOfGroups)
    {
        _intents.Add(CreateTokenIntent(intent, allOf, anyOfGroups));
    }

    [Given("the token catalog contains a specific intent \"(.*)\" with all tokens \"(.*)\" and any-of groups \"(.*)\"")]
    public void GivenTheTokenCatalogContainsSpecificIntent(string intent, string allOf, string anyOfGroups)
    {
        _intents.Add(CreateTokenIntent(intent, allOf, anyOfGroups));
    }

    [Given("the token catalog contains intent \"(.*)\" with example \"(.*)\" and follow-up \"(.*)\"")]
    public void GivenTheTokenCatalogContainsIntentWithExampleAndFollowUp(
        string intent,
        string example,
        string followUp)
    {
        _intents.Add(new IntentDefinition(
            intent,
            string.IsNullOrWhiteSpace(example) ? [] : [example],
            FollowUpExamples: string.IsNullOrWhiteSpace(followUp) ? [] : [followUp]));
    }

    [When("I classify the standalone token message \"(.*)\"")]
    public async Task WhenIClassifyTheTokenMessage(string message)
    {
        _result = await ClassifyAsync(message);
    }

    [When("I classify the token message \"(.*)\" using prior user message \"(.*)\"")]
    public async Task WhenIClassifyTheTokenMessageAfterPriorUserMessage(string message, string priorMessage)
    {
        _result = await ClassifyAsync(
            message,
            string.IsNullOrWhiteSpace(priorMessage) ? [] : [priorMessage]);
    }

    [Then("the token classified intent is \"(.*)\"")]
    public void ThenTheTokenClassifiedIntentIs(string expected)
    {
        if (expected.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            Assert.IsNull(_result);
            return;
        }

        Assert.IsNotNull(_result);
        Assert.AreEqual(expected, _result!.Intent.Name);
    }

    [Then("the token capture \"(.*)\" is \"(.*)\"")]
    public void ThenTheTokenCaptureIs(string captureName, string expected)
    {
        if (string.IsNullOrWhiteSpace(expected))
        {
            Assert.IsTrue(_result?.Captures is null || !_result.Captures.ContainsKey(captureName));
            return;
        }

        Assert.IsNotNull(_result?.Captures);
        Assert.AreEqual(expected, _result!.Captures![captureName]);
    }

    [Then("the token classification level is \"(.*)\"")]
    public void ThenTheTokenClassificationLevelIs(string expected)
    {
        if (expected.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            Assert.IsNull(_result);
            return;
        }

        Assert.IsNotNull(_result);
        Assert.IsNotNull(_result!.Intent);
    }

    private async Task<IntentMatch?> ClassifyAsync(
        string message,
        IReadOnlyList<string>? priorMessages = null)
    {
        var classifier = new RuleIntentClassifier(new IntentCatalog(_intents));
        return await classifier.ClassifyAsync(message, priorMessages, CancellationToken.None);
    }

    private static IntentDefinition CreateTokenIntent(
        string intent,
        string allOf,
        string anyOfGroups,
        string? noneOf = null,
        IReadOnlyList<PhraseCapture>? captures = null)
    {
        return new IntentDefinition(
            intent,
            [],
            TokenRule: new IntentTokenRule(
                ParseTokens(allOf),
                ParseGroups(anyOfGroups),
                ParseTokens(noneOf),
                captures));
    }

    private static IReadOnlyList<string> ParseTokens(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static IReadOnlyList<IReadOnlyList<string>> ParseGroups(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(group => (IReadOnlyList<string>)group.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .ToArray();
}
