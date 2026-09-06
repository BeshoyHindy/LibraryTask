using Lending.Domain;
using Microsoft.EntityFrameworkCore;

namespace Lending.Data;

public static class LendingSeed
{
    // Ids come from the database, so every read builds fresh entities rather than handing out
    // instances an earlier insert has already stamped.
    public static IReadOnlyList<Borrower> Borrowers =>
    [
        new() { Name = "Ada Whitfield" },
        new() { Name = "Nadia Osei" },
        new() { Name = "Bruno Castellanos" },
        new() { Name = "Dana Reinholt" },
        new() { Name = "Esther Vance" },
        new() { Name = "Farid Nasser" },
        new() { Name = "Greta Lindholm" },
        new() { Name = "Hugo Almeida" },
    ];

    public static IReadOnlyList<Loan> Loans =>
    [
        Closed(bookId: 4, borrowerId: 1, On(1, 6), On(1, 20)),
        Closed(bookId: 1, borrowerId: 1, On(1, 24), On(2, 2)),
        Closed(bookId: 2, borrowerId: 1, On(2, 3), On(2, 17)),
        Closed(bookId: 6, borrowerId: 1, On(2, 20), On(3, 6)),
        Closed(bookId: 12, borrowerId: 1, On(3, 18), On(4, 30)),
        Closed(bookId: 7, borrowerId: 1, On(3, 22), On(4, 20)),
        Closed(bookId: 10, borrowerId: 1, On(4, 18), On(5, 30)),
        Closed(bookId: 1, borrowerId: 1, On(5, 12), On(5, 20)),

        Closed(bookId: 3, borrowerId: 2, On(1, 9), On(1, 16)),
        Closed(bookId: 4, borrowerId: 2, On(2, 5), On(2, 26)),
        Closed(bookId: 2, borrowerId: 2, On(3, 2), On(3, 25)),
        Closed(bookId: 10, borrowerId: 2, On(3, 11), On(4, 15)),
        Closed(bookId: 9, borrowerId: 2, On(3, 27), On(4, 8)),
        Closed(bookId: 1, borrowerId: 2, On(4, 20), On(5, 1)),
        Closed(bookId: 7, borrowerId: 2, On(5, 6), On(6, 10)),

        Closed(bookId: 12, borrowerId: 3, On(1, 2), On(1, 28)),
        Closed(bookId: 2, borrowerId: 3, On(1, 12), On(2, 1)),
        Closed(bookId: 7, borrowerId: 3, On(2, 10), On(2, 26)),
        Closed(bookId: 4, borrowerId: 3, On(3, 5), On(3, 30)),
        Closed(bookId: 1, borrowerId: 3, On(3, 24), On(4, 14)),
        Closed(bookId: 5, borrowerId: 3, On(4, 26), On(5, 4)),
        Closed(bookId: 10, borrowerId: 3, On(6, 2), On(6, 25)),

        Closed(bookId: 7, borrowerId: 4, On(1, 15), On(2, 8)),
        Closed(bookId: 8, borrowerId: 4, On(1, 20), On(1, 30)),
        Closed(bookId: 10, borrowerId: 4, On(2, 6), On(3, 8)),
        Closed(bookId: 6, borrowerId: 4, On(3, 12), On(4, 19)),
        Closed(bookId: 2, borrowerId: 4, On(4, 2), On(4, 22)),
        Closed(bookId: 9, borrowerId: 4, On(5, 19), On(5, 28)),

        Closed(bookId: 12, borrowerId: 5, On(2, 12), On(3, 8)),
        Closed(bookId: 7, borrowerId: 5, On(3, 1), On(3, 20)),
        Closed(bookId: 1, borrowerId: 5, On(3, 16), On(3, 16)),
        Closed(bookId: 4, borrowerId: 5, On(3, 31), On(4, 24)),
        Closed(bookId: 2, borrowerId: 5, On(5, 8), On(5, 30)),

        Closed(bookId: 9, borrowerId: 6, On(1, 7), On(1, 21)),
        Closed(bookId: 3, borrowerId: 6, On(2, 18), On(2, 24)),
        Closed(bookId: 5, borrowerId: 6, On(3, 9), On(3, 21)),
        Closed(bookId: 6, borrowerId: 6, On(4, 28), On(5, 18)),
        Closed(bookId: 4, borrowerId: 6, On(5, 2), On(5, 26)),

        Loan.Open(bookId: 3, borrowerId: 7, On(6, 2)),
        Loan.Open(bookId: 11, borrowerId: 7, On(6, 5)),
    ];

    public static async Task ApplyAsync(LendingDbContext dbContext, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (await dbContext.Borrowers.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.Borrowers.AddRange(Borrowers);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.Loans.AddRange(Loans);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Loan Closed(int bookId, int borrowerId, DateOnly borrowedOn, DateOnly returnedOn)
    {
        var loan = Loan.Open(bookId, borrowerId, borrowedOn);
        var result = loan.Return(returnedOn);

        return result.IsSuccess
            ? loan
            : throw new InvalidOperationException($"Seed loan of book {bookId} by borrower {borrowerId} cannot be returned: {result.Error?.Message}");
    }

    private static DateOnly On(int month, int day) => new(2025, month, day);
}
