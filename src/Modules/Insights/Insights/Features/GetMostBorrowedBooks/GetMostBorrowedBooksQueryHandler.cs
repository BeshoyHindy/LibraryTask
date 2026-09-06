using Catalog.Contracts;
using Lending.Contracts;
using Library.Shared;
using Mediator;

namespace Insights.Features.GetMostBorrowedBooks;

public sealed class GetMostBorrowedBooksQueryHandler(ILoanReader loans, IBookReader books)
    : IQueryHandler<GetMostBorrowedBooksQuery, Result<IReadOnlyList<RankedBook>>>
{
    public async ValueTask<Result<IReadOnlyList<RankedBook>>> Handle(GetMostBorrowedBooksQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var counts = await loans.CountLoansPerBookAsync(query.From, query.To, cancellationToken);
        var summaries = await books.GetByIdsAsync([.. counts.Select(count => count.BookId)], cancellationToken);

        var top = RankedBook.Top(
            counts
                .Where(count => summaries.ContainsKey(count.BookId))
                .Select(count => RankedBook.Of(summaries[count.BookId], count.LoanCount)),
            LimitRule.OrDefault(query.Limit));

        return Result<IReadOnlyList<RankedBook>>.Success(top);
    }
}
