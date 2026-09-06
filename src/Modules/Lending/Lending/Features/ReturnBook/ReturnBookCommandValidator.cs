using FluentValidation;

namespace Lending.Features.ReturnBook;

public sealed class ReturnBookCommandValidator : AbstractValidator<ReturnBookCommand>
{
    public ReturnBookCommandValidator() => RuleFor(command => command.LoanId).GreaterThan(0);
}
