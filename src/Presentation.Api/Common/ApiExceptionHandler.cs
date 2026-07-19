namespace Goodtocode.AgentFramework.Presentation.Api.Common;

/// <summary>
/// Handles unhandled API exceptions and maps them to HTTP problem responses.
/// </summary>
public sealed class ApiExceptionHandler : IExceptionHandler
{
    /// <summary>
    /// Attempts to handle an exception for the current HTTP request.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns><c>true</c> when the exception has been handled.</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        IResult result = exception switch
        {
            CustomValidationException validationException =>
                (IResult)BuildValidationResult(validationException),
            CustomNotFoundException notFoundException =>
                (IResult)BuildNotFoundResult(notFoundException.Message),
            UnauthorizedAccessException => (IResult)BuildUnauthorizedResult(),
            CustomForbiddenAccessException => (IResult)BuildForbiddenResult(),
            CustomConflictException conflictException => (IResult)BuildConflictResult(conflictException.Message),
            _ => (IResult)BuildUnknownResult()
        };

        await result.ExecuteAsync(httpContext);
        return true;
    }

    private static BadRequest<ValidationProblemDetails> BuildValidationResult(CustomValidationException exception)
    {
        var details = new ValidationProblemDetails(exception.Errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        return TypedResults.BadRequest(details);
    }

    private static NotFound<ProblemDetails> BuildNotFoundResult(string detail)
    {
        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "The specified resource was not found.",
            Detail = detail
        };

        return TypedResults.NotFound(details);
    }

    private static JsonHttpResult<ProblemDetails> BuildUnauthorizedResult()
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        };

        return TypedResults.Json(details, statusCode: StatusCodes.Status401Unauthorized);
    }

    private static JsonHttpResult<ProblemDetails> BuildForbiddenResult()
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
        };

        return TypedResults.Json(details, statusCode: StatusCodes.Status403Forbidden);
    }

    private static JsonHttpResult<ProblemDetails> BuildConflictResult(string detail)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Detail = detail,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
        };

        return TypedResults.Json(details, statusCode: StatusCodes.Status409Conflict);
    }

    private static JsonHttpResult<ProblemDetails> BuildUnknownResult()
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        return TypedResults.Json(details, statusCode: StatusCodes.Status500InternalServerError);
    }
}
