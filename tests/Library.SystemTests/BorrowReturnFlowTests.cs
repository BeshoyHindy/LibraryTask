using System.Net;
using Library.Api.Insights;
using Library.Api.Lending;
using Xunit;

namespace Library.SystemTests;

// The flow mutates the database, so the class takes a clone of its own: borrower 5's seeded pace
// is 3050 pages over 90 days, and book 5 adds 134 pages over four.
public sealed class BorrowReturnFlowTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private const int BookId = 5;
    private const int BorrowerId = 5;

    private static readonly DateOnly BorrowedOn = new(2025, 7, 1);
    private static readonly DateOnly ReturnedOn = new(2025, 7, 5);

    [Fact]
    public async Task BorrowGetReturn_ThenReadingPace_CountsTheNewLoan()
    {
        var before = await ReadingPaceAsync();
        Assert.Equal(33.89, before.PagesPerDay);
        Assert.Equal(5, before.ClosedLoans);

        var created = await fixture.PostAsJsonAsync("/api/loans", new BorrowLoanRequest(BookId, BorrowerId, BorrowedOn));
        var borrowed = await created.ReadAsync<LoanResponse>(HttpStatusCode.Created);
        var location = created.Headers.Location;
        Assert.NotNull(location);
        Assert.Equal($"/api/loans/{borrowed.LoanId}", location.OriginalString);

        var fetchResponse = await fixture.GetAsync(location.OriginalString);
        var fetched = await fetchResponse.ReadAsync<LoanResponse>(HttpStatusCode.OK);
        Assert.Equal(borrowed, fetched);
        Assert.Null(fetched.ReturnedOn);

        var returnResponse = await fixture.PostAsJsonAsync($"/api/loans/{borrowed.LoanId}/return", new ReturnLoanRequest(ReturnedOn));
        var returned = await returnResponse.ReadAsync<LoanResponse>(HttpStatusCode.OK);
        Assert.Equal(ReturnedOn, returned.ReturnedOn);

        var after = await ReadingPaceAsync();
        Assert.Equal(33.87, after.PagesPerDay);
        Assert.Equal(6, after.ClosedLoans);
    }

    private async Task<ReadingPaceResponse> ReadingPaceAsync()
    {
        var response = await fixture.GetAsync($"/api/insights/borrowers/{BorrowerId}/reading-pace");

        return await response.ReadAsync<ReadingPaceResponse>(HttpStatusCode.OK);
    }
}
