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
        For every request that a registered tool can answer (chat sessions, chat messages, actors,
        or web search), call that tool immediately and deliver its result in this same reply.
        Never reply with only an announcement of intent such as "I will look that up", "Let me get
        that for you", "Querying...", or "One moment" - the user cannot see a follow-up turn, so an
        announcement without a delivered result is a failed response.
        Do not answer from memory or guess at data a tool would provide. Read-only queries execute
        immediately without confirmation. Ask for explicit confirmation only before a command that
        creates, changes, or deletes data.
        """;
}
