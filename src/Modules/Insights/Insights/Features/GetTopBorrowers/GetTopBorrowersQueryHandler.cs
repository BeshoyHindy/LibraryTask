using Lending.Contracts;
using Library.Shared;
using Mediator;

namespace Insights.Features.GetTopBorrowers;

public sealed class GetTopBorrowersQueryHandler(ILoanReader loans, IBorrowerReader borrowers)
    : IQueryHandler<GetTopBorrowersQuery, Result<IReadOnlyList<RankedBorrower>>>
{
    public async ValueTask<Result<IReadOnlyList<RankedBorrower>>> Handle(GetTopBorrowersQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var counts = await loans.CountLoansPerBorrowerAsync(query.From, query.To, cancellationToken);
        var summaries = await borrowers.GetByIdsAsync([.. counts.Select(count => count.BorrowerId)], cancellationToken);

        // Ties break on the name in ordinal order and then the id, as they do on the title in the
        // book rankings.
        var top = counts
            .Select(count => new RankedBorrower(count.BorrowerId, summaries[count.BorrowerId].Name, count.LoanCount))
            .OrderByDescending(borrower => borrower.LoanCount)
            .ThenBy(borrower => borrower.Name, StringComparer.Ordinal)
            .ThenBy(borrower => borrower.BorrowerId)
            .Take(LimitRule.OrDefault(query.Limit))
            .ToList();

        return Result<IReadOnlyList<RankedBorrower>>.Success(top);
    }
}
