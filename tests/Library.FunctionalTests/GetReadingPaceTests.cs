using Grpc.Core;
using Insights.Contracts.Grpc;
using Xunit;

namespace Library.FunctionalTests;

public sealed class GetReadingPaceTests(ServiceFixture fixture) : IClassFixture<ServiceFixture>
{
    private readonly InsightsService.InsightsServiceClient client = fixture.Host.CreateInsightsClient();

    // Borrower 5 closed five loans: 3050 pages over 90 days, rounded to two decimals here and
    // nowhere earlier.
    [Fact]
    public async Task GetReadingPace_ABorrowerWithClosedLoans_ReturnsTheirPagesPerDayToTwoDecimals()
    {
        var response = await client.GetReadingPaceAsync(
            new GetReadingPaceRequest { BorrowerId = 5 },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(5, response.BorrowerId);
        Assert.Equal(33.89, response.PagesPerDay);
        Assert.Equal(5, response.ClosedLoans);
    }

    [Fact]
    public async Task GetReadingPace_ABorrowerWithOnlyOpenLoans_HasNoPaceYet()
    {
        var response = await client.GetReadingPaceAsync(
            new GetReadingPaceRequest { BorrowerId = 7 },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(0, response.ClosedLoans);
        Assert.Equal(0, response.PagesPerDay);
    }

    [Fact]
    public async Task GetReadingPace_AnUnknownBorrower_IsNotFound()
    {
        var failure = await client
            .GetReadingPaceAsync(new GetReadingPaceRequest { BorrowerId = 99 }, cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.NotFound, failure.StatusCode);
    }
}
