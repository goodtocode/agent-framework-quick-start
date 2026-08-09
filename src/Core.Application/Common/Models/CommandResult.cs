namespace Goodtocode.AgentFramework.Core.Application.Common.Models;

public class CommandResult
{
    public bool IsSuccess { get; init; }
    public bool IsNotFound { get; init; }

    public static CommandResult Success() => new() { IsSuccess = true };
    public static CommandResult NotFound() => new() { IsNotFound = true };
}

public class CommandResult<T>
{
    public T? Value { get; init; }
    public bool IsNotFound { get; init; }

    public static CommandResult<T> Success(T value) => new() { Value = value };
    public static CommandResult<T> NotFound() => new() { IsNotFound = true };
}