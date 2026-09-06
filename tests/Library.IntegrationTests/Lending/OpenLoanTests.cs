using Lending.Data;
using Lending.Domain;
using Library.Migrations;
using Library.TestSupport;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Library.IntegrationTests.Lending;

public sealed class OpenLoanTests(TestDatabase database) : IClassFixture<TestDatabase>, IDisposable
{
    private readonly LendingDbContext dbContext = LendingDbContextFactory.Create(database.ConnectionString);

    public void Dispose() => dbContext.Dispose();

    [Fact]
    public async Task SaveChangesAsync_ASecondOpenLoanOnTheSameBook_IsRejected()
    {
        dbContext.Loans.Add(Loan.Open(bookId: 3, borrowerId: 8, new DateOnly(2025, 7, 1)));

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SaveChangesAsync_AnOpenLoanOnAReturnedBook_IsAccepted()
    {
        dbContext.Loans.Add(Loan.Open(bookId: 1, borrowerId: 8, new DateOnly(2025, 7, 1)));

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(41, await dbContext.Loans.CountAsync(TestContext.Current.CancellationToken));
    }
}
