using FluentValidation;

namespace Insights.Features.GetMostBorrowedBooks;

public sealed class GetMostBorrowedBooksQueryValidator : AbstractValidator<GetMostBorrowedBooksQuery>
{
    public GetMostBorrowedBooksQueryValidator()
    {
        RuleFor(query => query.Limit).IsALimit();

        // Either bound may be absent; only a range with both of them can be inverted.
        RuleFor(query => query.To)
            .GreaterThanOrEqualTo(query => query.From!.Value)
            .When(query => query is { From: not null, To: not null });
    }
}
