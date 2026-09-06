using Google.Type;
using Grpc.Core;
using Lending.Contracts.Grpc;
using Xunit;

namespace Library.FunctionalTests;

public sealed class ReturnBookTests(ServiceFixture fixture) : IClassFixture<ServiceFixture>
{
    // The seed leaves loan 1 closed and loan 39 open on book 3, borrowed on 2 June 2025.
    private const int ClosedLoanId = 1;
    private const int OpenSeedLoanId = 39;

    private static readonly Date ReturnedOn = Dates.On(2025, 7, 10);

    private readonly LendingService.LendingServiceClient client = fixture.Host.CreateLendingClient();

    [Fact]
    public async Task ReturnBook_AnOpenLoan_ReturnsTheClosedLoan()
    {
        var borrowed = await BorrowAsync();

        var loan = await client.ReturnBookAsync(
            Request(borrowed.LoanId, ReturnedOn),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(borrowed.LoanId, loan.LoanId);
        Assert.Equal(ReturnedOn, loan.ReturnedOn);
    }

    [Fact]
    public async Task ReturnBook_ALoanThatIsAlreadyClosed_IsFailedPrecondition()
    {
        var failure = await client
            .ReturnBookAsync(Request(ClosedLoanId, ReturnedOn), cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.FailedPrecondition, failure.StatusCode);
    }

    [Fact]
    public async Task ReturnBook_ADateBeforeTheBorrowDate_IsInvalidArgumentOnReturnedOn()
    {
        var failure = await client
            .ReturnBookAsync(Request(OpenSeedLoanId, Dates.On(2025, 6, 1)), cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.InvalidArgument, failure.StatusCode);
        Assert.Equal(["returnedOn"], failure.ViolatedFields());
    }

    [Fact]
    public async Task ReturnBook_AnUnknownLoan_IsNotFound()
    {
        var failure = await client
            .ReturnBookAsync(Request(999, ReturnedOn), cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.NotFound, failure.StatusCode);
    }

    [Fact]
    public async Task ReturnBook_ALoanIdThatIsNotPositive_IsInvalidArgumentOnLoanId()
    {
        var failure = await client
            .ReturnBookAsync(Request(0, ReturnedOn), cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.InvalidArgument, failure.StatusCode);
        Assert.Equal(["loanId"], failure.ViolatedFields());
    }

    [Fact]
    public async Task ReturnBook_WithoutAReturnDate_IsInvalidArgumentOnReturnedOn()
    {
        var failure = await client
            .ReturnBookAsync(new ReturnBookRequest { LoanId = OpenSeedLoanId }, cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.InvalidArgument, failure.StatusCode);
        Assert.Equal(["returnedOn"], failure.ViolatedFields());
    }

    private static ReturnBookRequest Request(int loanId, Date returnedOn) =>
        new() { LoanId = loanId, ReturnedOn = returnedOn };

    private Task<Loan> BorrowAsync() =>
        client.BorrowBookAsync(
            new BorrowBookRequest { BookId = 5, BorrowerId = 8, BorrowedOn = Dates.On(2025, 7, 1) },
            cancellationToken: TestContext.Current.CancellationToken).ResponseAsync;
}
