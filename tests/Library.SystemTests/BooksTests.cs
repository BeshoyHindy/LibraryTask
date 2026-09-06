using System.Net;
using Catalog.Data;
using Library.Api.Catalog;
using Xunit;

namespace Library.SystemTests;

public sealed class BooksTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task GetBooks_ReturnsEverySeededBookInIdOrder()
    {
        var response = await fixture.GetAsync("/api/books");

        var books = await response.ReadAsync<BookResponse[]>(HttpStatusCode.OK);
        Assert.Equal(Enumerable.Range(1, CatalogSeed.Books.Count), books.Select(book => book.BookId));
        Assert.Equal(CatalogSeed.Books.Select(book => book.Title), books.Select(book => book.Title));
    }
}
