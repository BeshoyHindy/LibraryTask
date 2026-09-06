using Google.Type;
using Grpc.Core;
using Lending.Contracts.Grpc;
using Xunit;

namespace Library.FunctionalTests;

public sealed class BorrowBookTests(ServiceFixture fixture) : IClassFixture<ServiceFixture>
{
    // Book 3 is on loan in the seed; book 5 is free, and this is the only test in the class that
    // borrows one, so the new loan continues the seed at id 41.
    private const int AvailableBookId = 5;
    private const int BookOnLoanId = 3;
    private const int BorrowerId = 8;

    private static readonly Date BorrowedOn = Dates.On(2025, 7, 1);

    private readonly LendingService.LendingServiceClient client = fixture.Host.CreateLendingClient();

    [Fact]
    public async Task BorrowBook_AnAvailableBook_ReturnsAnOpenLoanOnTheBorrowDate()
    {
        var loan = await client.BorrowBookAsync(
            Request(AvailableBookId, BorrowerId),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(41, loan.LoanId);
        Assert.Equal(AvailableBookId, loan.BookId);
        Assert.Equal(BorrowerId, loan.BorrowerId);
        Assert.Equal(BorrowedOn, loan.BorrowedOn);
        Assert.Null(loan.ReturnedOn);
    }

    [Fact]
    public async Task BorrowBook_ABookThatIsAlreadyOnLoan_IsFailedPrecondition()
    {
        var failure = await client
            .BorrowBookAsync(Request(BookOnLoanId, BorrowerId), cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.FailedPrecondition, failure.StatusCode);
    }

    [Fact]
    public async Task BorrowBook_AnUnknownBook_IsNotFound()
    {
        var failure = await client
            .BorrowBookAsync(Request(bookId: 99, BorrowerId), cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.NotFound, failure.StatusCode);
    }

    [Fact]
    public async Task BorrowBook_AnUnknownBorrower_IsNotFound()
    {
        var failure = await client
            .BorrowBookAsync(Request(AvailableBookId, borrowerId: 99), cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.NotFound, failure.StatusCode);
    }

    [Fact]
    public async Task BorrowBook_ABookIdThatIsNotPositive_IsInvalidArgumentOnBookId()
    {
        var failure = await client
            .BorrowBookAsync(Request(bookId: 0, BorrowerId), cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.InvalidArgument, failure.StatusCode);
        Assert.Equal(["bookId"], failure.ViolatedFields());
    }

    [Fact]
    public async Task BorrowBook_ABorrowerIdThatIsNotPositive_IsInvalidArgumentOnBorrowerId()
    {
        var failure = await client
            .BorrowBookAsync(Request(AvailableBookId, borrowerId: 0), cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.InvalidArgument, failure.StatusCode);
        Assert.Equal(["borrowerId"], failure.ViolatedFields());
    }

    [Fact]
    public async Task BorrowBook_WithoutABorrowDate_IsInvalidArgumentOnBorrowedOn()
    {
        var request = new BorrowBookRequest { BookId = AvailableBookId, BorrowerId = BorrowerId };

        var failure = await client
            .BorrowBookAsync(request, cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.InvalidArgument, failure.StatusCode);
        Assert.Equal(["borrowedOn"], failure.ViolatedFields());
    }

    private static BorrowBookRequest Request(int bookId, int borrowerId) =>
        new() { BookId = bookId, BorrowerId = borrowerId, BorrowedOn = BorrowedOn };
}
