namespace Goodtocode.AgentFramework.Core.Application.Chats.Journeys;

/// <summary>
/// A single suggested prompt offered to the customer. A suggested prompt is submitted through the
/// same message-input path as a typed prompt so the two remain interchangeable.
/// </summary>
public sealed record ChatJourneySuggestedPrompt(string Prompt);
