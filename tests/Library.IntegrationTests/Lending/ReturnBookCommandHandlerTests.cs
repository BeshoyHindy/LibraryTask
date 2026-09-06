using Lending.Data;
using Lending.Domain;
using Lending.Features.ReturnBook;
using Library.Migrations;
using Library.Shared;
using Library.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Library.IntegrationTests.Lending;

// Every test that returns a loan opens its own on a book of its own, so no test depends on what
// another one left behind.
public sealed class ReturnBookCommandHandlerTests : IClassFixture<TestDatabase>, IDisposable
{
    private const int ClosedSeedLoanId = 1;

    private static readonly DateOnly BorrowedOn = new(2025, 7, 1);
    private static readonly DateOnly ReturnedOn = new(2025, 7, 10);

    private readonly LendingDbContext dbContext;
    private readonly ReturnBookCommandHandler handler;

    public ReturnBookCommandHandlerTests(TestDatabase database)
    {
        dbContext = LendingDbContextFactory.Create(database.ConnectionString);
        handler = new ReturnBookCommandHandler(dbContext, NullLogger<ReturnBookCommandHandler>.Instance);
    }

    public void Dispose() => dbContext.Dispose();

    [Fact]
    public async Task Handle_AnOpenLoan_ClosesItOnTheReturnDate()
    {
        var loanId = await OpenLoanAsync(bookId: 5);

        var result = await handler.Handle(new ReturnBookCommand(loanId, ReturnedOn), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReturnedOn, result.Value.ReturnedOn);
    }

    [Fact]
    public async Task Handle_AnOpenLoan_SavesTheReturnDate()
    {
        var loanId = await OpenLoanAsync(bookId: 6);

        await handler.Handle(new ReturnBookCommand(loanId, ReturnedOn), TestContext.Current.CancellationToken);

        Assert.Equal(ReturnedOn, await ReturnedOnOfAsync(loanId));
    }

    [Fact]
    public async Task Handle_AClosedLoan_IsAConflict()
    {
        var result = await handler.Handle(
            new ReturnBookCommand(ClosedSeedLoanId, ReturnedOn),
            TestContext.Current.CancellationToken);

        Assert.Equal(ErrorKind.Conflict, result.Error?.Kind);
    }

    [Fact]
    public async Task Handle_AReturnBeforeTheBorrowDate_IsAValidationErrorOnReturnedOn()
    {
        var loanId = await OpenLoanAsync(bookId: 7);

        var result = await handler.Handle(
            new ReturnBookCommand(loanId, BorrowedOn.AddDays(-1)),
            TestContext.Current.CancellationToken);

        Assert.Equal(ErrorKind.Validation, result.Error?.Kind);
        Assert.Equal("returnedOn", Assert.Single(result.Error?.Fields ?? []).Field);
    }

    [Fact]
    public async Task Handle_AReturnBeforeTheBorrowDate_LeavesTheLoanOpen()
    {
        var loanId = await OpenLoanAsync(bookId: 8);

        await handler.Handle(new ReturnBookCommand(loanId, BorrowedOn.AddDays(-1)), TestContext.Current.CancellationToken);

        Assert.Null(await ReturnedOnOfAsync(loanId));
    }

    [Fact]
    public async Task Handle_AnUnknownLoan_IsNotFound()
    {
        var result = await handler.Handle(new ReturnBookCommand(999, ReturnedOn), TestContext.Current.CancellationToken);

        Assert.Equal(ErrorKind.NotFound, result.Error?.Kind);
    }

    private async Task<int> OpenLoanAsync(int bookId)
    {
        var loan = Loan.Open(bookId, borrowerId: 8, BorrowedOn);
        dbContext.Loans.Add(loan);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return loan.Id;
    }

    private async Task<DateOnly?> ReturnedOnOfAsync(int loanId)
    {
        var loan = await dbContext.Loans
            .AsNoTracking()
            .SingleAsync(loan => loan.Id == loanId, TestContext.Current.CancellationToken);

        return loan.ReturnedOn;
    }
}
