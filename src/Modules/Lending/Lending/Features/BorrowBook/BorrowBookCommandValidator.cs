using FluentValidation;

namespace Lending.Features.BorrowBook;

public sealed class BorrowBookCommandValidator : AbstractValidator<BorrowBookCommand>
{
    public BorrowBookCommandValidator()
    {
        RuleFor(command => command.BookId).GreaterThan(0);
        RuleFor(command => command.BorrowerId).GreaterThan(0);
    }
}
