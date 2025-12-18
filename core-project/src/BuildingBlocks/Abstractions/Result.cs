namespace BuildingBlocks.Abstractions;

/// <summary>
/// Represents a result that can either succeed or fail
/// </summary>
/// <typeparam name="T">The value type</typeparam>
public class Result<T>
{
    private Result(T? value, Error? error, bool isSuccess)
    {
        Value = value;
        Error = error;
        IsSuccess = isSuccess;
    }

    public T? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public static Result<T> Success(T value) => new(value, null, true);
    public static Result<T> Fail(string code, string message) => new(default, new Error(code, message), false);
}

/// <summary>
/// Represents an error with a code and message
/// </summary>
/// <param name="Code">The error code</param>
/// <param name="Message">The error message</param>
public record Error(string Code, string Message);