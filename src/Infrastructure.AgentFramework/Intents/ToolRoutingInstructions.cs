namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// Single source of truth for the "don't announce intent, call the tool now" instruction. This is
/// injected once into the system context message, so every current and future tool method gets the
/// same anti-hallucination guidance without any per-method repetition.
/// </summary>
public static class ToolRoutingInstructions
{
    /// <summary>
    /// Global anti-announcement instruction appended to the system context message. Forbids "I will
    /// look that up"/"Let me get that"/"Querying..." style replies in favor of calling tools
    /// immediately and returning results in the same turn.
    /// </summary>
    public const string AntiAnnouncementGuidance = """
        For every request that a registered tool can answer (chat sessions, chat messages, or
        actors), call that tool immediately and deliver its result in this same reply.
        Never reply with only an announcement of intent such as "I will look that up", "Let me get
        that for you", "Querying...", or "One moment" - the user cannot see a follow-up turn, so an
        announcement without a delivered result is a failed response.
        Do not answer from memory or guess at data a registered tool would provide (the user's chat
        sessions, chat messages, or actors). When no registered tool applies to a request, answer it
        directly and helpfully from your general knowledge instead of declining. Read-only queries execute
        immediately without confirmation. Ask for explicit confirmation only before a command that
        creates, changes, or deletes data.
        """;

    /// <summary>Added only when web search is available (feature switch on and provider configured).</summary>
    public const string WebSearchGuidance = """
        For requests to search the web, current news, or current public documentation, call the web
        search tool immediately and deliver its results in this same reply.
        """;

    /// <summary>Added only when web search is unavailable.</summary>
    public const string WebSearchUnavailableGuidance = """
        Live web search is not available. When a request needs public-web information, never refuse
        and never stop at saying search is unavailable. In the present tense, note once in a single
        short sentence that live results could not be retrieved and the answer comes from general
        knowledge that may be out of date, then give the most helpful complete answer you can from
        that knowledge.
        """;

    /// <summary>
    /// Mandatory preamble for every model inference. Mirrored by <see cref="FutureTenseGuard"/>, which
    /// validates every returned reply.
    /// </summary>
    public const string NoFutureTensePreamble = """
        ABSOLUTE RULE: Never use future tense or announce intended actions. Never write "I will", "I'll",
        "I am going to", "I'm going to", "let me", "allow me", "one moment", "please wait", "stand by",
        "I plan to", "I intend to", "next I", or any promise of later work. An action that has not
        already happened in this reply will never happen. Report only what is done, using past or
        present tense, or state plainly that the request cannot be completed.
        """;
}
