using Lending;
using Lending.Data;
using Library.Migrations;
using Library.TestSupport;
using Xunit;

namespace Library.IntegrationTests.Lending;

public sealed class BorrowerReaderTests : IClassFixture<TestDatabase>, IDisposable
{
    private readonly LendingDbContext dbContext;
    private readonly BorrowerReader reader;

    public BorrowerReaderTests(TestDatabase database)
    {
        dbContext = LendingDbContextFactory.Create(database.ConnectionString);
        reader = new BorrowerReader(dbContext);
    }

    public void Dispose() => dbContext.Dispose();

    [Fact]
    public async Task ExistsAsync_ASeededBorrower_IsTrue()
    {
        var exists = await reader.ExistsAsync(8, TestContext.Current.CancellationToken);

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_AnUnknownBorrower_IsFalse()
    {
        var exists = await reader.ExistsAsync(99, TestContext.Current.CancellationToken);

        Assert.False(exists);
    }

    [Fact]
    public async Task GetByIdsAsync_TheSeed_RunsFromOneToEightInOrder()
    {
        var summaries = await reader.GetByIdsAsync([.. Enumerable.Range(1, 8)], TestContext.Current.CancellationToken);

        Assert.Equal(8, summaries.Count);
        Assert.Equal("Ada Whitfield", summaries[1].Name);
        Assert.Equal("Hugo Almeida", summaries[8].Name);
    }

    [Fact]
    public async Task GetByIdsAsync_AnUnknownBorrower_IsLeftOut()
    {
        var summaries = await reader.GetByIdsAsync([5, 99], TestContext.Current.CancellationToken);

        Assert.Equal([5], summaries.Keys);
    }
}
