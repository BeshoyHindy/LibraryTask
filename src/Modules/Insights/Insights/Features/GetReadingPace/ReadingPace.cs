using Catalog.Contracts;
using Lending.Contracts;

namespace Insights.Features.GetReadingPace;

/// <summary>A borrower's pages per day over the loans they have closed.</summary>
public sealed record ReadingPace(int BorrowerId, double PagesPerDay, int ClosedLoans)
{
    // Total pages over total days, so a long loan weighs more than a short one. Lending counts a
    // same-day return as one day, so the divisor is at least the number of loans.
    public static ReadingPace Of(
        int borrowerId,
        IReadOnlyCollection<ClosedLoan> closedLoans,
        IReadOnlyDictionary<int, BookSummary> books)
    {
        ArgumentNullException.ThrowIfNull(closedLoans);
        ArgumentNullException.ThrowIfNull(books);

        var known = closedLoans.Where(loan => books.ContainsKey(loan.BookId)).ToList();
        if (known.Count == 0)
        {
            return new ReadingPace(borrowerId, 0, 0);
        }

        var pages = known.Sum(loan => books[loan.BookId].Pages);
        var days = known.Sum(loan => loan.Days);

        return new ReadingPace(borrowerId, (double)pages / days, known.Count);
    }
}
