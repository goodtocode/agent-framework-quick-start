namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// Declarative criteria for matching an intent against normalized message tokens.
/// Every <see cref="AllOf"/> token must be present, every <see cref="AnyOfGroups"/>
/// group must contribute at least one token, and <see cref="NoneOf"/> tokens reject
/// the match. Optional captures can extract structured values after the rule matches.
/// </summary>
public sealed record IntentTokenRule(
    IReadOnlyList<string> AllOf,
    IReadOnlyList<IReadOnlyList<string>> AnyOfGroups,
    IReadOnlyList<string>? NoneOf = null,
    IReadOnlyList<PhraseCapture>? Captures = null);
