namespace QFace.Sdk.Extensions;

/// <summary>
/// A simple Result class for representing operation results.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public class Result<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure { get; private set; }
    
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess => !IsFailure;
    
    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    public string Error { get; private set; }
    
    /// <summary>
    /// Gets the error code if the operation failed.
    /// </summary>
    public string Code { get; private set; }
    
    /// <summary>
    /// Gets the result value if the operation succeeded.
    /// </summary>
    public T Value { get; private set; }
    
    private Result(T value, bool isFailure, string error, string code)
    {
        Value = value;
        IsFailure = isFailure;
        Error = error;
        Code = code;
    }
    
    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The result value.</param>
    /// <returns>A successful result.</returns>
    public static Result<T> Ok(T value) => new Result<T>(value, false, string.Empty, string.Empty);
    
    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="code">The error code (default is "400").</param>
    /// <returns>A failed result.</returns>
    public static Result<T> Fail(string error, string code = "400") => new Result<T>(default, true, error, code);
    
    /// <summary>
    /// Creates a not found result.
    /// </summary>
    /// <param name="error">The error message (default is "Resource not found").</param>
    /// <returns>A not found result.</returns>
    public static Result<T> NotFound(string error = "Resource not found") => new Result<T>(default, true, error, "404");
}