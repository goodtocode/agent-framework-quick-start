namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// Declarative description of a single deterministic chat intent: its known-good phrasings
/// (<see cref="Examples"/>), any parameterized selection captures (<see cref="Captures"/>), and the
/// stable <see cref="Name"/> used by routing to dispatch to the matching handler. This is the single
/// source of truth for Level 1-4 phrasing guidance that prevents "I will look that up" hallucinations.
/// </summary>
/// <param name="Name">Stable identifier (see <see cref="IntentNames"/>) used for routing dispatch.</param>
/// <param name="Examples">
/// Known-good phrasings matched by substring (case-insensitive). Also the canonical place to add new
/// phrasing observed in production - iterative prompt tuning belongs here, not scattered across tool
/// descriptions/instructions.
/// </param>
/// <param name="Captures">
/// Optional <see cref="PhraseCapture"/> entries for parameterized intents (e.g. "select actor {id}").
/// A successful match contributes its value to <see cref="IntentMatch.Captures"/> under
/// <see cref="PhraseCapture.CaptureName"/>. Prefer this over hand-written regex for readability.
/// </param>
public sealed record IntentDefinition(
    string Name,
    IReadOnlyList<string> Examples,
    IReadOnlyList<PhraseCapture>? Captures = null);
