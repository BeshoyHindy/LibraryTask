using Insights.Features;
using Xunit;

namespace Library.UnitTests.Insights;

public sealed class RankedBookTests
{
    [Fact]
    public void Top_TiedCounts_OrdersByTitleThenId()
    {
        RankedBook[] books =
        [
            new(3, "Meridian Nine", "Sofia Marchetti", 5),
            new(2, "Almanac of Forgotten Roads", "Idris Haddad", 5),
            new(1, "Almanac of Forgotten Roads", "Idris Haddad", 5),
            new(4, "A Short History of Rain", "Priya Raman", 7),
        ];

        var top = RankedBook.Top(books, 10);

        Assert.Equal([4, 1, 2, 3], top.Select(book => book.BookId));
    }

    [Fact]
    public void Top_MoreBooksThanTheLimit_TakesTheLimit()
    {
        RankedBook[] books = [new(1, "A", "x", 3), new(2, "B", "x", 2), new(3, "C", "x", 1)];

        var top = RankedBook.Top(books, 2);

        Assert.Equal([1, 2], top.Select(book => book.BookId));
    }
}
