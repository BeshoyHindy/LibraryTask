namespace Library.Shared;

public readonly record struct Result<T> : IResult<Result<T>>
{
    private readonly T? value;

    private Result(T value)
    {
        this.value = value;
        Error = null;
    }

    private Result(Error error)
    {
        value = default;
        Error = error;
    }

    public bool IsSuccess => Error is null;

    public Error? Error { get; }

    public T Value => IsSuccess
        ? value!
        : throw new InvalidOperationException("A failed result carries no value.");

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error) => new(error);
}
