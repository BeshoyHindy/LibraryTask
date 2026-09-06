using FluentValidation;

namespace Insights.Features.GetTopBorrowers;

public sealed class GetTopBorrowersQueryValidator : AbstractValidator<GetTopBorrowersQuery>
{
    public GetTopBorrowersQueryValidator()
    {
        RuleFor(query => query.Limit).IsALimit();
        RuleFor(query => query.To).GreaterThanOrEqualTo(query => query.From);
    }
}
