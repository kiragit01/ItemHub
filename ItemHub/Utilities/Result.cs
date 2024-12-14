namespace ItemHub.Utilities;

public class Result<T>
{
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }
    public T Value { get; }

    protected Result(T value, bool isSuccess, string errorMessage)
    {
        Value = value;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Ok(T value) => new Result<T>(value, true, null!);
    public static Result<T> Fail(string errorMessage) => new Result<T>(default(T)!, false, errorMessage);
}
