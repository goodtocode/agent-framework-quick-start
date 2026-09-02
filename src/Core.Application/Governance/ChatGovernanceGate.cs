// This file requires Goodtocode.Agent.Governance which is not available.
// TODO: Implement governance using available dependencies.
using Goodtocode.AgentFramework.Core.Application.Abstractions;

namespace Goodtocode.AgentFramework.Core.Application.Governance;

/// <summary>
/// Placeholder for chat governance gate.
/// Requires Goodtocode.Agent.Governance dependency.
/// </summary>
public sealed class ChatGovernanceGate
{
    public class PromptContext
    {
        public string? SystemInstruction { get; set; }
    }

    public class GovernedEvaluationResult
    {
        public string? SystemInstruction { get; set; }
        public PromptContext? PromptContext { get; set; }
    }

    public GovernedEvaluationResult Enforce(
        IUserContext userContext,
        Guid chatSessionId,
        string prompt)
    {
        return new GovernedEvaluationResult
        {
            SystemInstruction = "You are a helpful AI assistant.",
            PromptContext = new PromptContext
            {
                SystemInstruction = "You are a helpful AI assistant."
            }
        };
    }
}
