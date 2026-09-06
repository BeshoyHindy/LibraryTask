namespace Library.Shared;

public readonly record struct Result : IResult<Result>
{
    private Result(Error? error) => Error = error;

    public bool IsSuccess => Error is null;

    public Error? Error { get; }

    public static Result Success() => new(error: null);

    public static Result Failure(Error error) => new(error);

    public static implicit operator Result(Error error) => new(error);
}
