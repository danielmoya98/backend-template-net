namespace BackendTemplate.Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public string? ErrorMessage => Error != Error.None ? Error.Description : null;

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A success result cannot have an error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(string errorMessage) => new(false, Error.Failure("General.Failure", errorMessage));

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);
    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);
    public static Result<TValue> Failure<TValue>(string errorMessage) => Result<TValue>.Failure(errorMessage);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    protected internal Result(bool isSuccess, TValue? value, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public static Result<TValue> Success(TValue value) =>
        new(true, value, Error.None);

    public new static Result<TValue> Failure(Error error) =>
        new(false, default, error);

    public new static Result<TValue> Failure(string errorMessage) =>
        new(false, default, Error.Failure("General.Failure", errorMessage));

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure(Error.NullValue);
}
