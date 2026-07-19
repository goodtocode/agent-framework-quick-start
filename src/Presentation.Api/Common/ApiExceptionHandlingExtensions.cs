namespace Goodtocode.AgentFramework.Presentation.Api.Common;

/// <summary>
/// Provides service registration helpers for API exception handling.
/// </summary>
public static class ApiExceptionHandlingExtensions
{
    /// <summary>
    /// Registers problem details and custom exception handling services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApiExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<ApiExceptionHandler>();
        return services;
    }
}
