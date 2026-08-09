using Goodtocode.AgentFramework.Core.Application.Common.Models;

namespace Goodtocode.AgentFramework.Presentation.Api.Common;

public static class ApiResponseMapper
{
    public static IResult SingleOrNotFound<T>(T? value)
    {
        return value is null ? TypedResults.NotFound() : TypedResults.Ok(value);
    }

    public static IResult ListOrOk<T>(IEnumerable<T>? values)
    {
        return TypedResults.Ok(values ?? Enumerable.Empty<T>());
    }

    public static IResult FromCommand(CommandResult result)
    {
        return result.IsNotFound ? TypedResults.NotFound() : TypedResults.NoContent();
    }

    public static IResult FromCommand<T>(CommandResult<T> result)
    {
        return result.IsNotFound || result.Value is null ? TypedResults.NotFound() : TypedResults.Ok(result.Value);
    }
}
