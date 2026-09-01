namespace Goodtocode.AgentFramework.Infrastructure.AgentFramework.Intents;

/// <summary>
/// Dispatches a classified <see cref="IntentMatch"/> to its concrete handler and returns the reply
/// text. The signature deliberately requires an <see cref="IntentMatch"/> rather than a raw
/// <c>string message</c> - since <see cref="IntentMatch"/> can only be constructed by
/// <see cref="IIntentClassifier.Classify"/>, this interface cannot be called without classification
/// having already happened.
/// </summary>
public interface IIntentRouter
{
    Task<string> RouteAsync(Guid chatSessionId, IntentMatch match, CancellationToken cancellationToken);
}
