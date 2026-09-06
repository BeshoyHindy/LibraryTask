using Lending.Contracts;
using Lending.Data;
using Microsoft.EntityFrameworkCore;

namespace Lending;

public sealed class BorrowerReader(LendingDbContext dbContext) : IBorrowerReader
{
    public Task<bool> ExistsAsync(int borrowerId, CancellationToken cancellationToken) =>
        dbContext.Borrowers.AnyAsync(borrower => borrower.Id == borrowerId, cancellationToken);

    public async Task<IReadOnlyDictionary<int, BorrowerSummary>> GetByIdsAsync(IReadOnlyCollection<int> borrowerIds, CancellationToken cancellationToken)
    {
        var summaries = await dbContext.Borrowers
            .AsNoTracking()
            .Where(borrower => borrowerIds.Contains(borrower.Id))
            .Select(borrower => new BorrowerSummary(borrower.Id, borrower.Name))
            .ToListAsync(cancellationToken);

        return summaries.ToDictionary(summary => summary.Id);
    }
}
