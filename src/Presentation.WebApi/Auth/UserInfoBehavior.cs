using Goodtocode.AgentFramework.Core.Application.Abstractions;
using Goodtocode.AgentFramework.Core.Domain.Auth;

namespace Goodtocode.AgentFramework.Presentation.WebApi.Auth;

/// <summary>
/// Pipeline behavior that automatically injects authenticated user context into requests
/// that implement <see cref="IRequiresUserContext"/>.
/// </summary>
/// <remarks>This behavior intercepts requests marked with <see cref="IRequiresUserContext"/> 
/// and populates the <see cref="IRequiresUserContext.UserContext"/> property with the current
/// authenticated user's context before invoking the request handler.</remarks>
/// <typeparam name="TRequest">The type of the request. Must implement <see cref="IRequiresUserContext"/>.</typeparam>
/// <param name="claimsReader">Service for reading claims from HTTP authentication context</param>
public class UserContextBehavior<TRequest>(IClaimsReader claimsReader) : IPipelineBehavior<TRequest>
       where TRequest : IRequiresUserContext
{
    /// <summary>
    /// Processes the request by injecting user context before invoking the next handler.
    /// </summary>
    /// <param name="request">The request to be processed.</param>
    /// <param name="nextInvoker">The delegate to invoke the next handler in the pipeline.</param>
    /// <param name="cancellationToken">A token that can be used to propagate notification that the operation should be canceled.</param>    
    public async Task Handle(TRequest request, RequestDelegateInvoker nextInvoker, CancellationToken cancellationToken)
    {
        request.UserContext = UserContext.Create(
            claimsReader.ObjectId, 
            claimsReader.TenantId, 
            claimsReader.FirstName, 
            claimsReader.LastName, 
            claimsReader.Email, 
            claimsReader.Roles);
        await nextInvoker();
    }
}

/// <summary>
/// Pipeline behavior that automatically injects authenticated user context into requests
/// that implement <see cref="IRequiresUserContext"/> and return a response.
/// </summary>
/// <remarks>This behavior intercepts requests marked with <see cref="IRequiresUserContext"/> 
/// and populates the <see cref="IRequiresUserContext.UserContext"/> property with the current
/// authenticated user's context before invoking the request handler.</remarks>
/// <typeparam name="TRequest">The type of the request. Must implement <see cref="IRequiresUserContext"/>.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <param name="claimsReader">Service for reading claims from HTTP authentication context</param>
public class UserContextBehavior<TRequest, TResponse>(IClaimsReader claimsReader) : IPipelineBehavior<TRequest, TResponse>
       where TRequest : IRequiresUserContext
{
    /// <summary>
    /// Processes the request by injecting user context before invoking the next handler.
    /// </summary>
    /// <param name="request">The request to be processed.</param>
    /// <param name="nextInvoker">The delegate to invoke the next handler in the pipeline.</param>
    /// <param name="cancellationToken">A token that can be used to propagate notification that the operation should be canceled.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response of type <typeparamref name="TResponse"/>.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestDelegateInvoker<TResponse> nextInvoker, CancellationToken cancellationToken)
    {
        request.UserContext = UserContext.Create(
            claimsReader.ObjectId, 
            claimsReader.TenantId, 
            claimsReader.FirstName, 
            claimsReader.LastName, 
            claimsReader.Email, 
            claimsReader.Roles);
        return await nextInvoker();
    }
}
