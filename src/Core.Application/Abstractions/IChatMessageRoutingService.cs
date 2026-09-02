namespace Goodtocode.AgentFramework.Core.Application.Abstractions;

/// <summary>
/// Resolves an assistant reply while keeping chat presentation, intent routing, and AI integration
/// outside application command and query handlers.
/// </summary>
public interface IChatMessageRoutingService
{
    /// <summary>
    /// Resolves the reply for <paramref name="message"/> in the specified chat session.
    /// </summary>
    Task<string> ResolveReplyAsync(
        Guid chatSessionId,
        string message,
        CancellationToken cancellationToken,
        ChatRoutingMode mode = ChatRoutingMode.Routed);
}