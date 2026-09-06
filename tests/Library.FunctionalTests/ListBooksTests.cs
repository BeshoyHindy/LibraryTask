using Catalog.Contracts.Grpc;
using Catalog.Data;
using Xunit;

namespace Library.FunctionalTests;

public sealed class ListBooksTests(ServiceFixture fixture) : IClassFixture<ServiceFixture>
{
    private readonly CatalogService.CatalogServiceClient client = fixture.Host.CreateCatalogClient();

    [Fact]
    public async Task ListBooks_ReturnsEverySeededBookInIdOrder()
    {
        var response = await client.ListBooksAsync(new ListBooksRequest(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(Enumerable.Range(1, CatalogSeed.Books.Count), response.Books.Select(book => book.BookId));
        Assert.Equal(CatalogSeed.Books.Select(book => book.Title), response.Books.Select(book => book.Title));
    }

    [Fact]
    public async Task ListBooks_CarriesAuthorAndPages()
    {
        var response = await client.ListBooksAsync(new ListBooksRequest(), cancellationToken: TestContext.Current.CancellationToken);

        var book = response.Books.Single(book => book.BookId == 7);
        Assert.Equal("Sofia Marchetti", book.Author);
        Assert.Equal(1024, book.Pages);
    }
}
