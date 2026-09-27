namespace Goodtocode.AgentFramework.Core.Application.Playbooks;

/// <summary>
/// Example Collect input: raw document content to review.
/// </summary>
public sealed record ReviewRequest(string Document);

/// <summary>
/// Example Collect output: the document plus any sources gathered for review.
/// </summary>
public sealed record ReviewEvidence(string Document, IReadOnlyList<string> Sources);

/// <summary>
/// Example Evaluate output: the review verdict and a human-readable summary.
/// </summary>
public sealed record ReviewFinding(bool Approved, string Summary);

/// <summary>
/// Example Record output: the persisted review status and summary.
/// </summary>
public sealed record ReviewRecord(string Status, string Summary);
