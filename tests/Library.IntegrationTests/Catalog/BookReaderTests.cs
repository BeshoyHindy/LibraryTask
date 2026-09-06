using Catalog;
using Catalog.Data;
using Library.Migrations;
using Library.TestSupport;
using Xunit;

namespace Library.IntegrationTests.Catalog;

public sealed class BookReaderTests : IClassFixture<TestDatabase>, IDisposable
{
    private readonly CatalogDbContext dbContext;
    private readonly BookReader reader;

    public BookReaderTests(TestDatabase database)
    {
        dbContext = CatalogDbContextFactory.Create(database.ConnectionString);
        reader = new BookReader(dbContext);
    }

    public void Dispose() => dbContext.Dispose();

    [Fact]
    public async Task ExistsAsync_ASeededBook_IsTrue()
    {
        var exists = await reader.ExistsAsync(1, TestContext.Current.CancellationToken);

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_AnUnknownBook_IsFalse()
    {
        var exists = await reader.ExistsAsync(99, TestContext.Current.CancellationToken);

        Assert.False(exists);
    }

    [Fact]
    public async Task GetByIdsAsync_ReturnsTitleAuthorAndPages()
    {
        var summaries = await reader.GetByIdsAsync([7], TestContext.Current.CancellationToken);

        var book = summaries[7];
        Assert.Equal("Meridian Nine", book.Title);
        Assert.Equal("Sofia Marchetti", book.Author);
        Assert.Equal(1024, book.Pages);
    }

    [Fact]
    public async Task GetByIdsAsync_TheSeed_RunsFromOneToTwelveInOrder()
    {
        var summaries = await reader.GetByIdsAsync([.. Enumerable.Range(1, 12)], TestContext.Current.CancellationToken);

        Assert.Equal(12, summaries.Count);
        Assert.Equal("The Silent Cartographer", summaries[1].Title);
        Assert.Equal("Almanac of Forgotten Roads", summaries[12].Title);
    }

    [Fact]
    public async Task GetByIdsAsync_AnUnknownBook_IsLeftOut()
    {
        var summaries = await reader.GetByIdsAsync([1, 99], TestContext.Current.CancellationToken);

        Assert.Equal([1], summaries.Keys);
    }
}
