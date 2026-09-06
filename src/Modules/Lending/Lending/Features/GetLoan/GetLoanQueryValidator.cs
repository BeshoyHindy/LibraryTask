using FluentValidation;

namespace Lending.Features.GetLoan;

// A loan id of zero is the caller's mistake, not a loan that cannot be found.
public sealed class GetLoanQueryValidator : AbstractValidator<GetLoanQuery>
{
    public GetLoanQueryValidator() => RuleFor(query => query.LoanId).GreaterThan(0);
}
