namespace QFace.Sdk.Extensions;

public class Result
{
    protected internal Result(bool isSuccess, Error? error = null, string message = "", string? code = null)
    {
        if (isSuccess && error != null)
        {
            throw new InvalidOperationException("A successful result cannot have an error.");
        }

        if (!isSuccess && error == null)
        {
            throw new InvalidOperationException("A failure result must have an error.");
        }

        IsSuccess = isSuccess;
        Error = isSuccess ? null : error;
        Message = string.IsNullOrEmpty(message) ? (isSuccess ? "Request Sent Successfully" : error!.Message) : message;
        Code = code ?? (isSuccess ? "200" : "500");
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    public string Message { get; }

    public string Code { get; }
    

    public static Result WithSuccess(string message = "Request Sent Successfully", string code = "200") =>
        new(true, null, message, code);

    public static Result<TValue> WithSuccess<TValue>(TValue value, string message = "Request Sent Successfully", string code = "200") =>
        new(value, true, null, message, code);

    public static Result WithFailure(Error error, string message = "", string code = "500") =>
        new(false, error, message, code);

    public static Result<TValue> WithFailure<TValue>(Error error, string message = "", string code = "500") =>
        new(default, false, error, message, code);  
    
    public static Result<TValue> WithNotFound<TValue>(Error error, string message = "Data Not Found", string code = "404") =>
        new(default, false, error, message, code);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error? error = null, string message = "", string? code = null)
        : base(isSuccess, error, message, code) =>
        _value = value;

    public TValue Data => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    public object ToResponse()
    {
        return new
        {
            Data = IsSuccess ? _value : default,
            IsSuccess,
            IsFailure,
            Error,
            Message,
            Code
        };
    }

    public static implicit operator Result<TValue>(TValue? value) => WithSuccess(value);
}

public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");
    public static readonly Error ConditionNotMet = new("Error.ConditionNotMet", "The specified condition was not met.");
}