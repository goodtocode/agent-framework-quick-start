using Goodtocode.AgentFramework.Core.Domain.Auth;
using Microsoft.AspNetCore.Authorization;

namespace Goodtocode.AgentFramework.Presentation.WebApi.Common;

/// <summary>
/// Sets up ISender Mediator property
/// </summary>
[ApiController]
[ApiExceptionFilter]
[Authorize]
//[Authorize(Roles = "User")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    /// <summary>
    /// Mediator property exposing ISender type
    /// </summary>
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Gets the user information associated with the current HTTP context.
    /// </summary>
    /// <remarks>The <see cref="IUserContext"/> instance is resolved from the dependency injection container
    /// using the current HTTP context's request services. Ensure that the required service  is registered in the
    /// application's service collection.</remarks>
    protected IUserContext UserContext => HttpContext.RequestServices.GetRequiredService<IUserContext>();
}
