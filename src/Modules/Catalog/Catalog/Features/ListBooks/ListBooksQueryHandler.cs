using Catalog.Contracts;
using Catalog.Data;
using Library.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Features.ListBooks;

public sealed class ListBooksQueryHandler(CatalogDbContext dbContext)
    : IQueryHandler<ListBooksQuery, Result<IReadOnlyList<BookSummary>>>
{
    public async ValueTask<Result<IReadOnlyList<BookSummary>>> Handle(ListBooksQuery query, CancellationToken cancellationToken)
    {
        var books = await dbContext.Books
            .AsNoTracking()
            .OrderBy(book => book.Id)
            .Select(book => new BookSummary(book.Id, book.Title, book.Author, book.Pages))
            .ToListAsync(cancellationToken);

        return books;
    }
}
