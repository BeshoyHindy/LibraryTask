using FluentValidation;

namespace Insights.Features.GetAlsoBorrowedBooks;

// A book id that no book carries is a book the caller cannot see, so it is a NotFound from the
// handler rather than a validation error; only the limit is checked here.
public sealed class GetAlsoBorrowedBooksQueryValidator : AbstractValidator<GetAlsoBorrowedBooksQuery>
{
    public GetAlsoBorrowedBooksQueryValidator() => RuleFor(query => query.Limit).IsALimit();
}
