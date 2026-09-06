using Catalog.Contracts;
using Lending.Contracts;
using Library.Shared;
using Mediator;

namespace Insights.Features.GetAlsoBorrowedBooks;

public sealed class GetAlsoBorrowedBooksQueryHandler(ILoanReader loans, IBookReader books)
    : IQueryHandler<GetAlsoBorrowedBooksQuery, Result<IReadOnlyList<RankedBook>>>
{
    public async ValueTask<Result<IReadOnlyList<RankedBook>>> Handle(GetAlsoBorrowedBooksQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!await books.ExistsAsync(query.BookId, cancellationToken))
        {
            return Error.NotFound($"Book {query.BookId} was not found.");
        }

        var counts = await loans.CountCoBorrowersAsync(query.BookId, cancellationToken);
        var summaries = await books.GetByIdsAsync([.. counts.Select(count => count.BookId)], cancellationToken);

        var top = RankedBook.Top(
            counts
                .Where(count => summaries.ContainsKey(count.BookId))
                .Select(count => RankedBook.Of(summaries[count.BookId], count.BorrowerCount)),
            LimitRule.OrDefault(query.Limit));

        return Result<IReadOnlyList<RankedBook>>.Success(top);
    }
}
