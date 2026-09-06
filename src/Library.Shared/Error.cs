namespace Library.Shared;

public sealed record Error(ErrorKind Kind, string Message, IReadOnlyList<FieldError> Fields)
{
    public static Error NotFound(string message) => new(ErrorKind.NotFound, message, []);

    public static Error Conflict(string message) => new(ErrorKind.Conflict, message, []);

    public static Error Validation(IReadOnlyList<FieldError> fields) =>
        new(ErrorKind.Validation, "Validation failed.", fields);
}
