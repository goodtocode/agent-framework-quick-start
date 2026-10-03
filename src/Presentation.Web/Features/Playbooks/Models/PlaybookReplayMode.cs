namespace Goodtocode.AgentFramework.Presentation.Web.Features.Playbooks.Models;

/// <summary>
/// UI-facing mirror of <c>Goodtocode.Agents.Playbook.Execution.PlaybookReplayMode</c>: which of
/// the three repeatability behaviors a re-execution should perform. Defined here - rather than
/// reusing the NSwag-generated client enum directly - because NJsonSchema cannot infer member
/// names from the backend's numeric enum values, so the generated type's members are unusable
/// placeholders (<c>_0</c>/<c>_1</c>/<c>_2</c>); this enum is cast to/from that generated type by
/// ordinal value in <see cref="Services.PlaybookService"/>, mirroring the same pattern already
/// used for <see cref="PlaybookCerStep"/>.
/// </summary>
public enum PlaybookReplayMode
{
    /// <summary>
    /// Fresh execution: run Collect, Evaluate, and Record.
    /// </summary>
    Rerun = 0,

    /// <summary>
    /// Rehydrate a prior execution's evidence and finding without running Collect or Evaluate;
    /// Record still runs so materialization can be re-rendered from the recalled finding.
    /// </summary>
    Recall = 1,

    /// <summary>
    /// Reuse prior evidence (skip Collect) and re-run Evaluate and Record against it, to verify
    /// exact reproduction of a governed result.
    /// </summary>
    Replay = 2
}

/// <summary>
/// Display helpers for <see cref="PlaybookReplayMode"/>, kept separate from the enum so error
/// messages read as natural phrases (e.g. "could not be rerun") instead of mechanically
/// appending "-ed" to the member name.
/// </summary>
public static class PlaybookReplayModeExtensions
{
    /// <summary>
    /// Returns the verb phrase describing this mode's action, for use after "could not" in an
    /// error message (e.g. "could not {ToVerbPhrase()}").
    /// </summary>
    public static string ToVerbPhrase(this PlaybookReplayMode mode) => mode switch
    {
        PlaybookReplayMode.Rerun => "be rerun",
        PlaybookReplayMode.Recall => "be recalled",
        PlaybookReplayMode.Replay => "be replayed",
        _ => "be executed"
    };
}
