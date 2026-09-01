namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// Result of a successful <see cref="IIntentClassifier.Classify"/> call. The constructor is
/// <c>internal</c> so that only code inside this assembly (i.e. <see cref="IIntentClassifier"/>
/// implementations) can produce one - callers outside <c>Infrastructure.AgentFramework</c> cannot
/// fabricate a match and invoke routing without classification having happened first. This is the
/// structural (compiler-enforced) guarantee that "classify before route" is followed.
/// </summary>
public sealed record IntentMatch
{
    internal IntentMatch(IntentDefinition intent, IReadOnlyDictionary<string, string>? captures = null)
    {
        Intent = intent;
        Captures = captures;
    }

    /// <summary>The matched intent definition.</summary>
    public IntentDefinition Intent { get; }

    /// <summary>
    /// Named capture groups from a <see cref="IntentDefinition.Captures"/> match, if the match
    /// came from a parameterized pattern rather than a plain phrase.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Captures { get; }
}
