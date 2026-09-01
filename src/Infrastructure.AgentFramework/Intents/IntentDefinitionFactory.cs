namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// Factory helpers that turn the proven "known-good phrasing bypasses the LLM's own tool selection"
/// fix pattern into one-liners, so hardening a new tool method no longer requires hand-writing a
/// <see cref="PhraseCapture"/> + <see cref="IntentDefinition"/> from scratch each time.
/// </summary>
public static class IntentDefinitionFactory
{
    /// <summary>
    /// Builds a deterministic intent that matches "{keyword} {guid}" anywhere in the message (e.g.
    /// "actor {id}") and exposes the GUID under <paramref name="captureName"/>.
    /// </summary>
    /// <param name="name">Stable intent name (add a matching constant to <see cref="IntentNames"/>).</param>
    /// <param name="keyword">The literal word/phrase immediately preceding the GUID, e.g. "actor ". Include the trailing space.</param>
    /// <param name="captureName">Key under which the GUID is exposed in <see cref="IntentMatch.Captures"/>.</param>
    public static IntentDefinition ByIdKeyword(string name, string keyword, string captureName = "id") =>
        new(name, Examples: [], Captures: [new PhraseCapture(keyword, captureName, CaptureKind.GuidDFormat)]);

    /// <summary>
    /// Builds a deterministic intent that matches a literal prefix phrase and captures the free-text
    /// remainder of the message, e.g. "search the web for {query}".
    /// </summary>
    public static IntentDefinition ByFreeTextSuffix(string name, string prefix, string captureName) =>
        new(name, Examples: [], Captures: [new PhraseCapture(prefix, captureName, CaptureKind.Rest)]);

    /// <summary>
    /// Builds a deterministic intent from a set of known-good, parameter-free phrasings (case
    /// insensitive substring match), e.g. "list my chat sessions".
    /// </summary>
    public static IntentDefinition ByPhrases(string name, params string[] examples) =>
        new(name, Examples: examples);
}
