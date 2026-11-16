using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Goodtocode.AgentStart.Presentation.Web.Infrastructure.Http;

/// <summary>
/// Translates failures from Presentation.Api calls into messages that are safe to show to the user.
/// </summary>
public static class ApiErrorMessage
{
    public static string For(Exception exception) => exception switch
    {
        TimeoutRejectedException or TaskCanceledException or TimeoutException
            => "This is taking longer than expected. Please try again in a moment.",
        BrokenCircuitException
            => "The service is temporarily unavailable. Please try again shortly.",
        HttpRequestException
            => "The service could not be reached. Please try again.",
        _ => "Something went wrong. Please try again."
    };
}
