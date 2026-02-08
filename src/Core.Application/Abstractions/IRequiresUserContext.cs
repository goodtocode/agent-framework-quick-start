using Goodtocode.AgentFramework.Core.Domain.Auth;

namespace Goodtocode.AgentFramework.Core.Application.Abstractions;

/// <summary>
/// Marker interface for requests that require user context to be injected via pipeline behavior.
/// </summary>
/// <remarks>This interface is used to identify requests that need authenticated user information.
/// When a request implements this interface, the UserInfoBehavior pipeline will automatically
/// populate the <see cref="UserContext"/> property with the current user's context before
/// the request handler executes.</remarks>
public interface IRequiresUserContext
{
    /// <summary>
    /// Gets or sets the authenticated user's context.
    /// </summary>
    /// <remarks>This property is automatically populated by the pipeline behavior
    /// before the request handler is invoked.</remarks>
    IUserContext? UserContext { get; set; }
}
