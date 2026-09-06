using FluentValidation;
using Mediator;

namespace Library.Shared;

public sealed class ValidationBehaviour<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
    where TResponse : IResult<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(message, cancellationToken);
            if (!result.IsValid)
            {
                var fields = result.Errors
                    .Select(failure => new FieldError(FieldNameOf(failure.PropertyName), failure.ErrorMessage))
                    .ToList();

                return TResponse.Failure(Error.Validation(fields));
            }
        }

        return await next(message, cancellationToken);
    }

    // Callers see the field under the name they sent it, which is camelCase on the wire and in
    // JSON, not the PascalCase property name FluentValidation reports.
    private static string FieldNameOf(string propertyName) =>
        string.IsNullOrEmpty(propertyName)
            ? propertyName
            : char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
}
