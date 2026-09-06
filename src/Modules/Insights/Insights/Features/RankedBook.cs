using Catalog.Contracts;

namespace Insights.Features;

/// <summary>A book in one of the two book rankings, with the count it was ranked by.</summary>
public sealed record RankedBook(int BookId, string Title, string Author, int Count)
{
    public static RankedBook Of(BookSummary book, int count)
    {
        ArgumentNullException.ThrowIfNull(book);

        return new RankedBook(book.Id, book.Title, book.Author, count);
    }

    // Both rankings break ties the same way, on the title in ordinal order and then the id, so the
    // answer does not depend on the order the database returned the groups in.
    public static IReadOnlyList<RankedBook> Top(IEnumerable<RankedBook> books, int limit) =>
    [
        .. books
            .OrderByDescending(book => book.Count)
            .ThenBy(book => book.Title, StringComparer.Ordinal)
            .ThenBy(book => book.BookId)
            .Take(limit)
    ];
}
