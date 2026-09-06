using Grpc.Core;
using Lending.Contracts.Grpc;
using Xunit;

namespace Library.FunctionalTests;

public sealed class GetLoanTests(ServiceFixture fixture) : IClassFixture<ServiceFixture>
{
    // Loan 1 of the seed is book 4 by borrower 1, borrowed on 6 January 2025 and returned on the
    // 20th; loan 39 is the open loan of book 3 by borrower 7.
    private const int ClosedLoanId = 1;
    private const int OpenLoanId = 39;

    private readonly LendingService.LendingServiceClient client = fixture.Host.CreateLendingClient();

    [Fact]
    public async Task GetLoan_AClosedLoan_CarriesBothDates()
    {
        var loan = await client.GetLoanAsync(
            new GetLoanRequest { LoanId = ClosedLoanId },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(4, loan.BookId);
        Assert.Equal(1, loan.BorrowerId);
        Assert.Equal(Dates.On(2025, 1, 6), loan.BorrowedOn);
        Assert.Equal(Dates.On(2025, 1, 20), loan.ReturnedOn);
    }

    [Fact]
    public async Task GetLoan_AnOpenLoan_HasNoReturnDate()
    {
        var loan = await client.GetLoanAsync(
            new GetLoanRequest { LoanId = OpenLoanId },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(3, loan.BookId);
        Assert.Equal(Dates.On(2025, 6, 2), loan.BorrowedOn);
        Assert.Null(loan.ReturnedOn);
    }

    [Fact]
    public async Task GetLoan_AnUnknownLoan_IsNotFound()
    {
        var failure = await client
            .GetLoanAsync(new GetLoanRequest { LoanId = 999 }, cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.NotFound, failure.StatusCode);
    }

    [Fact]
    public async Task GetLoan_ALoanIdThatIsNotPositive_IsInvalidArgumentOnLoanId()
    {
        var failure = await client
            .GetLoanAsync(new GetLoanRequest { LoanId = 0 }, cancellationToken: TestContext.Current.CancellationToken)
            .FailureAsync();

        Assert.Equal(StatusCode.InvalidArgument, failure.StatusCode);
        Assert.Equal(["loanId"], failure.ViolatedFields());
    }
}
