using Goodtocode.AgentFramework.Core.Application.Common.Models;

namespace Goodtocode.AgentFramework.Presentation.Api.Common;

/// <summary>
/// Maps application-layer query and command results to consistent HTTP API responses.
/// </summary>
public static class ApiResponseMapper
{
    /// <summary>
    /// Returns <c>200 OK</c> with <paramref name="value"/> when present; otherwise returns <c>404 Not Found</c>.
    /// </summary>
    /// <typeparam name="T">The response payload type.</typeparam>
    /// <param name="value">The query result value.</param>
    /// <returns>An <see cref="IResult"/> representing either OK or NotFound.</returns>
    public static IResult SingleOrNotFound<T>(T? value)
    {
        return value is null ? TypedResults.NotFound() : TypedResults.Ok(value);
    }

    /// <summary>
    /// Returns <c>200 OK</c> with the provided list, or an empty list when <paramref name="values"/> is null.
    /// </summary>
    /// <typeparam name="T">The list item type.</typeparam>
    /// <param name="values">The collection to return.</param>
    /// <returns>An <see cref="IResult"/> containing a non-null collection payload.</returns>
    public static IResult ListOrOk<T>(IEnumerable<T>? values)
    {
        return TypedResults.Ok(values ?? Enumerable.Empty<T>());
    }

    /// <summary>
    /// Maps a non-generic command result to <c>204 No Content</c> on success or <c>404 Not Found</c> when missing.
    /// </summary>
    /// <param name="result">The command execution result.</param>
    /// <returns>An <see cref="IResult"/> representing the command outcome.</returns>
    public static IResult FromCommand(CommandResult result)
    {
        return result.IsNotFound ? TypedResults.NotFound() : TypedResults.NoContent();
    }

    /// <summary>
    /// Maps a generic command result to <c>200 OK</c> with payload on success, or <c>404 Not Found</c> when missing.
    /// </summary>
    /// <typeparam name="T">The command payload type.</typeparam>
    /// <param name="result">The command execution result with payload.</param>
    /// <returns>An <see cref="IResult"/> representing the command outcome.</returns>
    public static IResult FromCommand<T>(CommandResult<T> result)
    {
        return result.IsNotFound || result.Value is null ? TypedResults.NotFound() : TypedResults.Ok(result.Value);
    }
}
