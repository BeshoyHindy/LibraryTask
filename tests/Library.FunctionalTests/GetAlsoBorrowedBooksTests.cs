using Grpc.Core;
using Insights.Contracts.Grpc;
using Xunit;

namespace Library.FunctionalTests;

public sealed class GetAlsoBorrowedBooksTests(ServiceFixture fixture) : IClassFixture<ServiceFixture>
{
    private readonly InsightsService.InsightsServiceClient client = fixture.Host.CreateInsightsClient();

    [Fact]
    public async Task GetAlsoBorrowedBooks_ABorrowedBook_RanksTheOtherBooksItsBorrowersTook()
    {
        var response = await client.GetAlsoBorrowedBooksAsync(
            new GetAlsoBorrowedBooksRequest { BookId = 4 },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([7, 2, 1, 12, 10, 3, 5, 6, 9], response.Books.Select(book => book.BookId));
        Assert.Equal([4, 4, 4, 3, 3, 2, 2, 2, 2], response.Books.Select(book => book.BorrowerCount));
    }

    [Fact]
    public async Task GetAlsoBorrowedBooks_ALimit_TakesThatMany()
    {
        var response = await client.GetAlsoBorrowedBooksAsync(
            new GetAlsoBorrowedBooksRequest { BookId = 4, Limit = 2 },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([7, 2], response.Books.Select(book => book.BookId));
        Assert.Equal("Meridian Nine", response.Books[0].Title);
    }

    [Fact]
    public async Task GetAlsoBorrowedBooks_AnUnknownBook_IsNotFound()
    {
        var failure = await client
            .GetAlsoBorrowedBooksAsync(
                new GetAlsoBorrowedBooksRequest { BookId = 99 },
                cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.NotFound, failure.StatusCode);
    }

    [Fact]
    public async Task GetAlsoBorrowedBooks_ALimitAboveAHundred_IsInvalidArgumentOnLimit()
    {
        var failure = await client
            .GetAlsoBorrowedBooksAsync(
                new GetAlsoBorrowedBooksRequest { BookId = 4, Limit = 101 },
                cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.InvalidArgument, failure.StatusCode);
        Assert.Equal(["limit"], failure.ViolatedFields());
    }
}
