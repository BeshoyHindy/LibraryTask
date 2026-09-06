using Catalog;
using Catalog.Data;
using Lending.Data;
using Lending.Features.BorrowBook;
using Library.Migrations;
using Library.Shared;
using Library.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Library.IntegrationTests.Lending;

// Book 3 is the only seeded book with an open loan; every other book id is free for one test to
// borrow, which keeps the tests independent of the order they run in.
public sealed class BorrowBookCommandHandlerTests : IClassFixture<TestDatabase>, IDisposable
{
    private const int BookOnLoan = 3;
    private const int BorrowerId = 8;

    private static readonly DateOnly BorrowedOn = new(2025, 7, 1);

    private readonly LendingDbContext lendingDbContext;
    private readonly CatalogDbContext catalogDbContext;
    private readonly BorrowBookCommandHandler handler;

    public BorrowBookCommandHandlerTests(TestDatabase database)
    {
        lendingDbContext = LendingDbContextFactory.Create(database.ConnectionString);
        catalogDbContext = CatalogDbContextFactory.Create(database.ConnectionString);
        handler = new BorrowBookCommandHandler(
            lendingDbContext,
            new BookReader(catalogDbContext),
            NullLogger<BorrowBookCommandHandler>.Instance);
    }

    public void Dispose()
    {
        lendingDbContext.Dispose();
        catalogDbContext.Dispose();
    }

    [Fact]
    public async Task Handle_AnAvailableBook_OpensALoanOnTheBorrowDate()
    {
        var result = await handler.Handle(Command(bookId: 5), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.BookId);
        Assert.Equal(BorrowerId, result.Value.BorrowerId);
        Assert.Equal(BorrowedOn, result.Value.BorrowedOn);
        Assert.Null(result.Value.ReturnedOn);
    }

    [Fact]
    public async Task Handle_AnAvailableBook_SavesTheLoan()
    {
        var result = await handler.Handle(Command(bookId: 6), TestContext.Current.CancellationToken);

        var loan = await lendingDbContext.Loans
            .AsNoTracking()
            .SingleAsync(loan => loan.Id == result.Value.Id, TestContext.Current.CancellationToken);
        Assert.Equal(6, loan.BookId);
        Assert.Null(loan.ReturnedOn);
    }

    [Fact]
    public async Task Handle_AnUnknownBook_IsNotFound()
    {
        var result = await handler.Handle(Command(bookId: 99), TestContext.Current.CancellationToken);

        Assert.Equal(ErrorKind.NotFound, result.Error?.Kind);
    }

    [Fact]
    public async Task Handle_AnUnknownBorrower_IsNotFound()
    {
        var result = await handler.Handle(
            new BorrowBookCommand(BookId: 7, BorrowerId: 99, BorrowedOn),
            TestContext.Current.CancellationToken);

        Assert.Equal(ErrorKind.NotFound, result.Error?.Kind);
    }

    [Fact]
    public async Task Handle_ABookThatIsAlreadyOnLoan_IsAConflict()
    {
        var result = await handler.Handle(Command(BookOnLoan), TestContext.Current.CancellationToken);

        Assert.Equal(ErrorKind.Conflict, result.Error?.Kind);
    }

    [Fact]
    public async Task Handle_ABookThatIsAlreadyOnLoan_SavesNothing()
    {
        await handler.Handle(Command(BookOnLoan), TestContext.Current.CancellationToken);

        var borrowed = await lendingDbContext.Loans
            .AsNoTracking()
            .AnyAsync(loan => loan.BookId == BookOnLoan && loan.BorrowerId == BorrowerId, TestContext.Current.CancellationToken);
        Assert.False(borrowed);
    }

    private static BorrowBookCommand Command(int bookId) => new(bookId, BorrowerId, BorrowedOn);
}
