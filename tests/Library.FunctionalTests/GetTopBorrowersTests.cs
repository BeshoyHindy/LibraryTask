using Google.Type;
using Grpc.Core;
using Insights.Contracts.Grpc;
using Xunit;

namespace Library.FunctionalTests;

public sealed class GetTopBorrowersTests(ServiceFixture fixture) : IClassFixture<ServiceFixture>
{
    private static readonly Date From = Dates.On(2025, 3, 1);
    private static readonly Date To = Dates.On(2025, 3, 31);

    private readonly InsightsService.InsightsServiceClient client = fixture.Host.CreateInsightsClient();

    [Fact]
    public async Task GetTopBorrowers_ARange_RanksBorrowersByTheirLoansInIt()
    {
        var response = await client.GetTopBorrowersAsync(Request(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([5, 2, 1, 3, 4, 6], response.Borrowers.Select(borrower => borrower.BorrowerId));
        Assert.Equal([3, 3, 2, 2, 1, 1], response.Borrowers.Select(borrower => borrower.LoanCount));
    }

    [Fact]
    public async Task GetTopBorrowers_ARankedBorrower_CarriesTheirName()
    {
        var response = await client.GetTopBorrowersAsync(Request(limit: 1), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Esther Vance", Assert.Single(response.Borrowers).Name);
    }

    [Fact]
    public async Task GetTopBorrowers_WithoutAFromDate_IsInvalidArgumentOnFrom()
    {
        var failure = await client
            .GetTopBorrowersAsync(new GetTopBorrowersRequest { To = To }, cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.InvalidArgument, failure.StatusCode);
        Assert.Equal(["from"], failure.ViolatedFields());
    }

    [Fact]
    public async Task GetTopBorrowers_WithoutAToDate_IsInvalidArgumentOnTo()
    {
        var failure = await client
            .GetTopBorrowersAsync(new GetTopBorrowersRequest { From = From }, cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.InvalidArgument, failure.StatusCode);
        Assert.Equal(["to"], failure.ViolatedFields());
    }

    [Fact]
    public async Task GetTopBorrowers_AnInvertedRange_IsInvalidArgumentOnTo()
    {
        var request = new GetTopBorrowersRequest { From = To, To = From };

        var failure = await client
            .GetTopBorrowersAsync(request, cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.InvalidArgument, failure.StatusCode);
        Assert.Equal(["to"], failure.ViolatedFields());
    }

    [Fact]
    public async Task GetTopBorrowers_ALimitAboveAHundred_IsInvalidArgumentOnLimit()
    {
        var failure = await client
            .GetTopBorrowersAsync(Request(limit: 101), cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.InvalidArgument, failure.StatusCode);
        Assert.Equal(["limit"], failure.ViolatedFields());
    }

    // A limit of 0 is the proto's way of leaving it out.
    private static GetTopBorrowersRequest Request(int limit = 0) => new() { From = From, To = To, Limit = limit };
}
