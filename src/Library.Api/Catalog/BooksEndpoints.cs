using Catalog.Contracts.Grpc;

namespace Library.Api.Catalog;

public static class BooksEndpoints
{
    public static void MapBooksEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var books = endpoints.MapGroup("/api/books").WithTags("Books");

        books.MapGet("/", async (
            CatalogService.CatalogServiceClient catalog,
            CancellationToken cancellationToken) =>
        {
            var response = await catalog.ListBooksAsync(new ListBooksRequest(), cancellationToken: cancellationToken);

            return response.Books
                .Select(book => new BookResponse(book.BookId, book.Title, book.Author, book.Pages))
                .ToArray();
        }).WithSummary("List every book in the catalogue.");
    }
}
