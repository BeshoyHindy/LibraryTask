using System.Net;
using Library.Api.Insights;
using Xunit;

namespace Library.SystemTests;

public sealed class InsightsTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private const string March = "from=2025-03-01&to=2025-03-31";

    [Fact]
    public async Task GetMostBorrowedBooks_WithoutALimit_ReturnsTheTenMostBorrowedBooks()
    {
        var response = await fixture.GetAsync("/api/insights/most-borrowed-books");

        var books = await response.ReadAsync<MostBorrowedBookResponse[]>(HttpStatusCode.OK);
        Assert.Equal([7, 2, 4, 1, 10, 3, 12, 6, 9, 5], books.Select(book => book.BookId));
        Assert.Equal([5, 5, 5, 5, 4, 3, 3, 3, 3, 2], books.Select(book => book.LoanCount));
        Assert.Equal("Meridian Nine", books[0].Title);
        Assert.Equal("Sofia Marchetti", books[0].Author);
    }

    [Fact]
    public async Task GetMostBorrowedBooks_ARange_CountsOnlyLoansBorrowedInIt()
    {
        var response = await fixture.GetAsync($"/api/insights/most-borrowed-books?{March}");

        var books = await response.ReadAsync<MostBorrowedBookResponse[]>(HttpStatusCode.OK);
        Assert.Equal([7, 4, 1, 12, 10, 5, 2, 6, 9], books.Select(book => book.BookId));
    }

    [Fact]
    public async Task GetMostBorrowedBooks_ALimitAboveAHundred_Returns400OnLimit()
    {
        var response = await fixture.GetAsync("/api/insights/most-borrowed-books?limit=101");

        var errors = await response.ValidationErrorsAsync();
        Assert.Equal(["limit"], errors.Keys);
        Assert.NotEmpty(errors["limit"]);
    }

    [Fact]
    public async Task GetTopBorrowers_ARange_RanksBorrowersByTheirLoansInIt()
    {
        var response = await fixture.GetAsync($"/api/insights/top-borrowers?{March}");

        var borrowers = await response.ReadAsync<TopBorrowerResponse[]>(HttpStatusCode.OK);
        Assert.Equal([5, 2, 1, 3, 4, 6], borrowers.Select(borrower => borrower.BorrowerId));
        Assert.Equal([3, 3, 2, 2, 1, 1], borrowers.Select(borrower => borrower.LoanCount));
        Assert.Equal("Esther Vance", borrowers[0].Name);
    }

    [Fact]
    public async Task GetTopBorrowers_ALimit_TakesThatMany()
    {
        var response = await fixture.GetAsync($"/api/insights/top-borrowers?{March}&limit=2");

        var borrowers = await response.ReadAsync<TopBorrowerResponse[]>(HttpStatusCode.OK);
        Assert.Equal([5, 2], borrowers.Select(borrower => borrower.BorrowerId));
    }

    [Fact]
    public async Task GetTopBorrowers_WithoutAFromDate_Returns400OnFrom()
    {
        var response = await fixture.GetAsync("/api/insights/top-borrowers?to=2025-03-31");

        var errors = await response.ValidationErrorsAsync();
        Assert.Equal(["from"], errors.Keys);
    }

    [Fact]
    public async Task GetTopBorrowers_AnInvertedRange_Returns400OnTo()
    {
        var response = await fixture.GetAsync("/api/insights/top-borrowers?from=2025-03-31&to=2025-03-01");

        var errors = await response.ValidationErrorsAsync();
        Assert.Equal(["to"], errors.Keys);
    }

    [Fact]
    public async Task GetReadingPace_ABorrowerWithClosedLoans_ReturnsTheirPagesPerDay()
    {
        var response = await fixture.GetAsync("/api/insights/borrowers/5/reading-pace");

        var pace = await response.ReadAsync<ReadingPaceResponse>(HttpStatusCode.OK);
        Assert.Equal(5, pace.BorrowerId);
        Assert.Equal(33.89, pace.PagesPerDay);
        Assert.Equal(5, pace.ClosedLoans);
    }

    [Fact]
    public async Task GetReadingPace_ABorrowerWithOnlyOpenLoans_HasNoPaceYet()
    {
        var response = await fixture.GetAsync("/api/insights/borrowers/7/reading-pace");

        var pace = await response.ReadAsync<ReadingPaceResponse>(HttpStatusCode.OK);
        Assert.Equal(0, pace.ClosedLoans);
        Assert.Equal(0, pace.PagesPerDay);
    }

    [Fact]
    public async Task GetAlsoBorrowedBooks_ABorrowedBook_RanksTheOtherBooksItsBorrowersTook()
    {
        var response = await fixture.GetAsync("/api/insights/books/4/also-borrowed");

        var books = await response.ReadAsync<AlsoBorrowedBookResponse[]>(HttpStatusCode.OK);
        Assert.Equal([7, 2, 1, 12, 10, 3, 5, 6, 9], books.Select(book => book.BookId));
        Assert.Equal([4, 4, 4, 3, 3, 2, 2, 2, 2], books.Select(book => book.BorrowerCount));
    }

    [Fact]
    public async Task GetAlsoBorrowedBooks_ALimit_TakesThatMany()
    {
        var response = await fixture.GetAsync("/api/insights/books/4/also-borrowed?limit=2");

        var books = await response.ReadAsync<AlsoBorrowedBookResponse[]>(HttpStatusCode.OK);
        Assert.Equal([7, 2], books.Select(book => book.BookId));
        Assert.Equal("Meridian Nine", books[0].Title);
    }

    [Fact]
    public async Task GetAlsoBorrowedBooks_AnUnknownBook_Returns404()
    {
        var response = await fixture.GetAsync("/api/insights/books/99/also-borrowed");

        var problem = await response.ProblemAsync(HttpStatusCode.NotFound);
        Assert.Equal("Book 99 was not found.", problem.Detail);
    }
}
