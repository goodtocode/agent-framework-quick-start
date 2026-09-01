namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// The kind of value a <see cref="PhraseCapture"/> extracts after its literal prefix phrase.
/// </summary>
public enum CaptureKind
{
    /// <summary>A GUID in "D" format (32 hex digits separated by dashes), e.g. from "select actor {id}".</summary>
    GuidDFormat,

    /// <summary>A single token of letters/digits/<c>_</c>/<c>.</c>/<c>:</c>/<c>-</c>, e.g. a code.</summary>
    Word,

    /// <summary>Everything remaining after the prefix, trimmed, e.g. a free-text search query.</summary>
    Rest
}
