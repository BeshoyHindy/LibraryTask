using Catalog.Contracts;
using Catalog.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog;

public sealed class BookReader(CatalogDbContext dbContext) : IBookReader
{
    public Task<bool> ExistsAsync(int bookId, CancellationToken cancellationToken) =>
        dbContext.Books.AnyAsync(book => book.Id == bookId, cancellationToken);

    public async Task<IReadOnlyDictionary<int, BookSummary>> GetByIdsAsync(IReadOnlyCollection<int> bookIds, CancellationToken cancellationToken)
    {
        var summaries = await dbContext.Books
            .AsNoTracking()
            .Where(book => bookIds.Contains(book.Id))
            .Select(book => new BookSummary(book.Id, book.Title, book.Author, book.Pages))
            .ToListAsync(cancellationToken);

        return summaries.ToDictionary(summary => summary.Id);
    }
}
