using Lending.Data;
using Lending.Domain;
using Library.Migrations;
using Library.TestSupport;
using Xunit;

namespace Library.IntegrationTests.Lending;

public sealed class LoanIdentityTests(TestDatabase database) : IClassFixture<TestDatabase>, IDisposable
{
    private readonly LendingDbContext dbContext = LendingDbContextFactory.Create(database.ConnectionString);

    public void Dispose() => dbContext.Dispose();

    [Fact]
    public async Task SaveChangesAsync_TheFirstLoanAfterTheSeed_TakesIdFortyOne()
    {
        var loan = Loan.Open(bookId: 1, borrowerId: 8, new DateOnly(2025, 7, 1));
        dbContext.Loans.Add(loan);

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(41, loan.Id);
    }
}
