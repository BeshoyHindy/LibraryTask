namespace Lending.Contracts;

public interface IBorrowerReader
{
    Task<bool> ExistsAsync(int borrowerId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<int, BorrowerSummary>> GetByIdsAsync(IReadOnlyCollection<int> borrowerIds, CancellationToken cancellationToken);
}
