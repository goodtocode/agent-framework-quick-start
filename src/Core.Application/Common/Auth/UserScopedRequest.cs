namespace Goodtocode.AgentFramework.Core.Application.Common.Auth;

/// <summary>
/// Abstract base class for requests that require a user context.
/// Ensures that a valid <see cref="IUserContext"/> is injected and provides access to it.
/// </summary>
public abstract class UserScopedRequest : IRequiresUserContext
{
    private IUserContext? _userContext;

    /// <summary>
    /// Gets the injected <see cref="IUserContext"/>. Throws if not set.
    /// </summary>
    public IUserContext UserContext
        => _userContext ?? new UserContext();

    /// <summary>
    /// Gets or sets the <see cref="IUserContext"/> for the request. Throws if set more than once or if set to null.
    /// </summary>
    IUserContext IRequiresUserContext.UserContext
    {
        get => UserContext;
        set
        {
            if (_userContext is not null)
                throw new CustomConflictException("UserContext already set.");
            _userContext = value ?? throw new CustomValidationException(
                [new(nameof(UserContext), "UserContext cannot be null.")]);
        }
    }
}
