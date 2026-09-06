using Catalog.Contracts;
using Lending.Contracts;
using Library.Shared;
using Mediator;

namespace Insights.Features.GetReadingPace;

public sealed class GetReadingPaceQueryHandler(ILoanReader loans, IBorrowerReader borrowers, IBookReader books)
    : IQueryHandler<GetReadingPaceQuery, Result<ReadingPace>>
{
    public async ValueTask<Result<ReadingPace>> Handle(GetReadingPaceQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!await borrowers.ExistsAsync(query.BorrowerId, cancellationToken))
        {
            return Error.NotFound($"Borrower {query.BorrowerId} was not found.");
        }

        var closedLoans = await loans.ClosedLoansOfAsync(query.BorrowerId, cancellationToken);

        // A borrower can take the same book twice, so the batch asks for each book once.
        var summaries = await books.GetByIdsAsync(
            [.. closedLoans.Select(loan => loan.BookId).Distinct()],
            cancellationToken);

        return ReadingPace.Of(query.BorrowerId, closedLoans, summaries);
    }
}
