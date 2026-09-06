using System.Net;
using Library.Api.Lending;
using Xunit;

namespace Library.SystemTests;

public sealed class LoansTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    // The seed leaves loan 1 closed and book 3 on loan as loan 39, borrowed on 2 June 2025. Each
    // test that borrows takes a book of its own, so none of them depends on the order they run in.
    private const int ClosedLoanId = 1;
    private const int BookOnLoanId = 3;
    private const int OpenSeedLoanId = 39;
    private const int BorrowerId = 8;

    private static readonly DateOnly BorrowedOn = new(2025, 7, 1);
    private static readonly DateOnly ReturnedOn = new(2025, 7, 10);

    [Fact]
    public async Task PostLoans_AnAvailableBook_Returns201WithTheLoanAndItsLocation()
    {
        var response = await BorrowAsync(bookId: 5);

        var loan = await response.ReadAsync<LoanResponse>(HttpStatusCode.Created);
        Assert.Equal(5, loan.BookId);
        Assert.Equal(BorrowerId, loan.BorrowerId);
        Assert.Equal(BorrowedOn, loan.BorrowedOn);
        Assert.Null(loan.ReturnedOn);
        Assert.Equal($"/api/loans/{loan.LoanId}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task PostLoans_ABookThatIsAlreadyOnLoan_Returns409()
    {
        var response = await BorrowAsync(BookOnLoanId);

        var problem = await response.ProblemAsync(HttpStatusCode.Conflict);
        Assert.Equal($"Book {BookOnLoanId} is already on loan.", problem.Detail);
    }

    [Fact]
    public async Task PostLoans_AnUnknownBook_Returns404()
    {
        var response = await BorrowAsync(bookId: 99);

        var problem = await response.ProblemAsync(HttpStatusCode.NotFound);
        Assert.Equal("Book 99 was not found.", problem.Detail);
    }

    [Fact]
    public async Task PostLoans_ABookIdThatIsNotPositive_Returns400OnBookId()
    {
        var response = await BorrowAsync(bookId: 0);

        var errors = await response.ValidationErrorsAsync();
        Assert.Equal(["bookId"], errors.Keys);
    }

    [Fact]
    public async Task PostLoanReturn_AnOpenLoan_Returns200WithTheReturnDate()
    {
        var borrowResponse = await BorrowAsync(bookId: 6);
        var borrowed = await borrowResponse.ReadAsync<LoanResponse>(HttpStatusCode.Created);

        var response = await ReturnAsync(borrowed.LoanId, ReturnedOn);

        var loan = await response.ReadAsync<LoanResponse>(HttpStatusCode.OK);
        Assert.Equal(borrowed.LoanId, loan.LoanId);
        Assert.Equal(ReturnedOn, loan.ReturnedOn);
    }

    [Fact]
    public async Task PostLoanReturn_ALoanThatIsAlreadyClosed_Returns409()
    {
        var response = await ReturnAsync(ClosedLoanId, ReturnedOn);

        await response.ProblemAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostLoanReturn_ADateBeforeTheBorrowDate_Returns400OnReturnedOn()
    {
        var response = await ReturnAsync(OpenSeedLoanId, new DateOnly(2025, 6, 1));

        var errors = await response.ValidationErrorsAsync();
        Assert.Equal(["returnedOn"], errors.Keys);
    }

    [Fact]
    public async Task GetLoan_AClosedSeedLoan_Returns200WithBothDates()
    {
        var response = await fixture.GetAsync($"/api/loans/{ClosedLoanId}");

        var loan = await response.ReadAsync<LoanResponse>(HttpStatusCode.OK);
        Assert.Equal(4, loan.BookId);
        Assert.Equal(1, loan.BorrowerId);
        Assert.Equal(new DateOnly(2025, 1, 6), loan.BorrowedOn);
        Assert.Equal(new DateOnly(2025, 1, 20), loan.ReturnedOn);
    }

    [Fact]
    public async Task GetLoan_AnUnknownLoan_Returns404()
    {
        var response = await fixture.GetAsync("/api/loans/999");

        var problem = await response.ProblemAsync(HttpStatusCode.NotFound);
        Assert.Equal("Loan 999 was not found.", problem.Detail);
    }

    private Task<HttpResponseMessage> BorrowAsync(int bookId) =>
        fixture.PostAsJsonAsync("/api/loans", new BorrowLoanRequest(bookId, BorrowerId, BorrowedOn));

    private Task<HttpResponseMessage> ReturnAsync(int loanId, DateOnly returnedOn) =>
        fixture.PostAsJsonAsync($"/api/loans/{loanId}/return", new ReturnLoanRequest(returnedOn));
}
