using Catalog.Contracts;
using Insights.Features.GetReadingPace;
using Lending.Contracts;
using Xunit;

namespace Library.UnitTests.Insights;

public sealed class ReadingPaceTests
{
    private static readonly IReadOnlyDictionary<int, BookSummary> Books = new Dictionary<int, BookSummary>
    {
        [1] = new(1, "A Short History of Rain", "Priya Raman", 300),
        [2] = new(2, "Salt and Longitude", "Henrik Vasser", 450),
    };

    [Fact]
    public void Of_SeveralLoans_DividesTotalPagesByTotalDays()
    {
        var pace = ReadingPace.Of(5, [new ClosedLoan(1, 10), new ClosedLoan(2, 15)], Books);

        Assert.Equal(30d, pace.PagesPerDay);
    }

    [Fact]
    public void Of_SeveralLoans_CountsThem()
    {
        var pace = ReadingPace.Of(5, [new ClosedLoan(1, 10), new ClosedLoan(2, 15)], Books);

        Assert.Equal(5, pace.BorrowerId);
        Assert.Equal(2, pace.ClosedLoans);
    }

    [Fact]
    public void Of_TheSameBookTwice_CountsItsPagesTwice()
    {
        var pace = ReadingPace.Of(5, [new ClosedLoan(1, 5), new ClosedLoan(1, 5)], Books);

        Assert.Equal(60d, pace.PagesPerDay);
    }

    // Lending reports a same-day return as one day, so the shortest loan a borrower can have
    // contributes its whole page count to a single day rather than dividing by zero.
    [Fact]
    public void Of_ASameDayLoan_CountsItAsOneDay()
    {
        var pace = ReadingPace.Of(5, [new ClosedLoan(1, 1)], Books);

        Assert.Equal(300d, pace.PagesPerDay);
    }

    [Fact]
    public void Of_AnUnevenDivision_KeepsTheFullPrecision()
    {
        var pace = ReadingPace.Of(5, [new ClosedLoan(1, 9)], Books);

        Assert.Equal(300d / 9, pace.PagesPerDay);
    }
}
