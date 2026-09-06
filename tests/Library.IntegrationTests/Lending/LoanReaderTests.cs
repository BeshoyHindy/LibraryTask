using Lending;
using Lending.Contracts;
using Lending.Data;
using Library.Migrations;
using Library.TestSupport;
using Xunit;

namespace Library.IntegrationTests.Lending;

public sealed class LoanReaderTests : IClassFixture<TestDatabase>, IDisposable
{
    private readonly LendingDbContext dbContext;
    private readonly LoanReader reader;

    public LoanReaderTests(TestDatabase database)
    {
        dbContext = LendingDbContextFactory.Create(database.ConnectionString);
        reader = new LoanReader(dbContext);
    }

    public void Dispose() => dbContext.Dispose();

    [Fact]
    public async Task CountLoansPerBookAsync_WithNoRange_CountsEverySeededLoan()
    {
        var counts = await reader.CountLoansPerBookAsync(null, null, TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                new BookLoanCount(1, 5),
                new BookLoanCount(2, 5),
                new BookLoanCount(3, 3),
                new BookLoanCount(4, 5),
                new BookLoanCount(5, 2),
                new BookLoanCount(6, 3),
                new BookLoanCount(7, 5),
                new BookLoanCount(8, 1),
                new BookLoanCount(9, 3),
                new BookLoanCount(10, 4),
                new BookLoanCount(11, 1),
                new BookLoanCount(12, 3),
            ],
            counts.OrderBy(count => count.BookId));
    }

    [Fact]
    public async Task CountLoansPerBookAsync_WithARange_CountsOnlyLoansBorrowedInIt()
    {
        var counts = await reader.CountLoansPerBookAsync(
            new DateOnly(2025, 3, 1),
            new DateOnly(2025, 3, 31),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                new BookLoanCount(1, 2),
                new BookLoanCount(2, 1),
                new BookLoanCount(4, 2),
                new BookLoanCount(5, 1),
                new BookLoanCount(6, 1),
                new BookLoanCount(7, 2),
                new BookLoanCount(9, 1),
                new BookLoanCount(10, 1),
                new BookLoanCount(12, 1),
            ],
            counts.OrderBy(count => count.BookId));
    }

    [Fact]
    public async Task CountLoansPerBookAsync_TheRangeBounds_AreInclusive()
    {
        var borrowedOnTheBound = new DateOnly(2025, 6, 2);

        var counts = await reader.CountLoansPerBookAsync(
            borrowedOnTheBound,
            borrowedOnTheBound,
            TestContext.Current.CancellationToken);

        Assert.Equal([new BookLoanCount(3, 1), new BookLoanCount(10, 1)], counts.OrderBy(count => count.BookId));
    }

    [Fact]
    public async Task CountLoansPerBorrowerAsync_CountsLoansBorrowedInTheRange()
    {
        var counts = await reader.CountLoansPerBorrowerAsync(
            new DateOnly(2025, 3, 1),
            new DateOnly(2025, 3, 31),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                new BorrowerLoanCount(1, 2),
                new BorrowerLoanCount(2, 3),
                new BorrowerLoanCount(3, 2),
                new BorrowerLoanCount(4, 1),
                new BorrowerLoanCount(5, 3),
                new BorrowerLoanCount(6, 1),
            ],
            counts.OrderBy(count => count.BorrowerId));
    }

    [Fact]
    public async Task CountCoBorrowersAsync_CountsDistinctBorrowersPerOtherBook()
    {
        var counts = await reader.CountCoBorrowersAsync(4, TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                new BookCoBorrowerCount(1, 4),
                new BookCoBorrowerCount(2, 4),
                new BookCoBorrowerCount(3, 2),
                new BookCoBorrowerCount(5, 2),
                new BookCoBorrowerCount(6, 2),
                new BookCoBorrowerCount(7, 4),
                new BookCoBorrowerCount(9, 2),
                new BookCoBorrowerCount(10, 3),
                new BookCoBorrowerCount(12, 3),
            ],
            counts.OrderBy(count => count.BookId));
    }

    [Fact]
    public async Task CountCoBorrowersAsync_LeavesOutTheBookItself()
    {
        var counts = await reader.CountCoBorrowersAsync(4, TestContext.Current.CancellationToken);

        Assert.DoesNotContain(counts, count => count.BookId == 4);
    }

    [Fact]
    public async Task ClosedLoansOfAsync_ReturnsEachClosedLoanWithItsDayCount()
    {
        var closedLoans = await reader.ClosedLoansOfAsync(5, TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                new ClosedLoan(1, 1),
                new ClosedLoan(2, 22),
                new ClosedLoan(4, 24),
                new ClosedLoan(7, 19),
                new ClosedLoan(12, 24),
            ],
            closedLoans.OrderBy(loan => loan.BookId));
    }

    [Fact]
    public async Task ClosedLoansOfAsync_ABorrowerWithOnlyOpenLoans_IsEmpty()
    {
        var closedLoans = await reader.ClosedLoansOfAsync(7, TestContext.Current.CancellationToken);

        Assert.Empty(closedLoans);
    }

    [Fact]
    public async Task ClosedLoansOfAsync_ABorrowerWithNoLoans_IsEmpty()
    {
        var closedLoans = await reader.ClosedLoansOfAsync(8, TestContext.Current.CancellationToken);

        Assert.Empty(closedLoans);
    }
}
