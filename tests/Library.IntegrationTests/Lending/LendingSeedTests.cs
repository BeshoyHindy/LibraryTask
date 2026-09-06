using Lending.Data;
using Library.Migrations;
using Library.TestSupport;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Library.IntegrationTests.Lending;

public sealed class LendingSeedTests(TestDatabase database) : IClassFixture<TestDatabase>, IDisposable
{
    private readonly LendingDbContext dbContext = LendingDbContextFactory.Create(database.ConnectionString);

    public void Dispose() => dbContext.Dispose();

    [Fact]
    public async Task ApplyAsync_LeavesEightBorrowersAndFortyLoans()
    {
        var borrowers = await dbContext.Borrowers.CountAsync(TestContext.Current.CancellationToken);
        var loans = await dbContext.Loans.CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(8, borrowers);
        Assert.Equal(40, loans);
    }

    [Fact]
    public async Task ApplyAsync_LeavesLoanIdsRunningFromOneToForty()
    {
        var ids = await dbContext.Loans
            .OrderBy(loan => loan.Id)
            .Select(loan => loan.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(Enumerable.Range(1, 40), ids);
    }

    [Fact]
    public async Task ApplyAsync_OnAnAlreadySeededLending_InsertsNothing()
    {
        await LendingSeed.ApplyAsync(dbContext, TestContext.Current.CancellationToken);

        Assert.Equal(40, await dbContext.Loans.CountAsync(TestContext.Current.CancellationToken));
    }
}
