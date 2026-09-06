using Google.Type;
using Grpc.Core;
using Insights.Contracts.Grpc;
using Xunit;

namespace Library.FunctionalTests;

public sealed class GetMostBorrowedBooksTests(ServiceFixture fixture) : IClassFixture<ServiceFixture>
{
    private readonly InsightsService.InsightsServiceClient client = fixture.Host.CreateInsightsClient();

    [Fact]
    public async Task GetMostBorrowedBooks_WithoutALimit_ReturnsTheTenMostBorrowedBooks()
    {
        var response = await client.GetMostBorrowedBooksAsync(
            new GetMostBorrowedBooksRequest(),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([7, 2, 4, 1, 10, 3, 12, 6, 9, 5], response.Books.Select(book => book.BookId));
        Assert.Equal([5, 5, 5, 5, 4, 3, 3, 3, 3, 2], response.Books.Select(book => book.LoanCount));
    }

    [Fact]
    public async Task GetMostBorrowedBooks_ARankedBook_CarriesItsTitleAndAuthor()
    {
        var response = await client.GetMostBorrowedBooksAsync(
            new GetMostBorrowedBooksRequest { Limit = 1 },
            cancellationToken: TestContext.Current.CancellationToken);

        var book = Assert.Single(response.Books);
        Assert.Equal("Meridian Nine", book.Title);
        Assert.Equal("Sofia Marchetti", book.Author);
    }

    [Fact]
    public async Task GetMostBorrowedBooks_ARange_CountsOnlyLoansBorrowedInIt()
    {
        var request = new GetMostBorrowedBooksRequest { From = Dates.On(2025, 3, 1), To = Dates.On(2025, 3, 31) };

        var response = await client.GetMostBorrowedBooksAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([7, 4, 1, 12, 10, 5, 2, 6, 9], response.Books.Select(book => book.BookId));
    }

    [Fact]
    public async Task GetMostBorrowedBooks_ALimitAboveAHundred_IsInvalidArgumentOnLimit()
    {
        var failure = await client
            .GetMostBorrowedBooksAsync(
                new GetMostBorrowedBooksRequest { Limit = 101 },
                cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.InvalidArgument, failure.StatusCode);
        Assert.Equal(["limit"], failure.ViolatedFields());
    }

    [Fact]
    public async Task GetMostBorrowedBooks_AnInvertedRange_IsInvalidArgumentOnTo()
    {
        var request = new GetMostBorrowedBooksRequest { From = Dates.On(2025, 3, 31), To = Dates.On(2025, 3, 1) };

        var failure = await client
            .GetMostBorrowedBooksAsync(request, cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.InvalidArgument, failure.StatusCode);
        Assert.Equal(["to"], failure.ViolatedFields());
    }

    [Fact]
    public async Task GetMostBorrowedBooks_APartialFromDate_IsInvalidArgumentOnFrom()
    {
        var request = new GetMostBorrowedBooksRequest { From = new Date { Year = 2025 } };

        var failure = await client
            .GetMostBorrowedBooksAsync(request, cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.InvalidArgument, failure.StatusCode);
        Assert.Equal(["from"], failure.ViolatedFields());
    }
}
