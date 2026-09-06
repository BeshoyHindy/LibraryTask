namespace Catalog.Contracts;

public interface IBookReader
{
    Task<bool> ExistsAsync(int bookId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<int, BookSummary>> GetByIdsAsync(IReadOnlyCollection<int> bookIds, CancellationToken cancellationToken);
}
