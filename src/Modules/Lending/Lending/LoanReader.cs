using Lending.Contracts;
using Lending.Data;
using Microsoft.EntityFrameworkCore;

namespace Lending;

public sealed class LoanReader(LendingDbContext dbContext) : ILoanReader
{
    public async Task<IReadOnlyList<BookLoanCount>> CountLoansPerBookAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        var loans = dbContext.Loans.AsNoTracking();

        if (from is DateOnly start)
        {
            loans = loans.Where(loan => loan.BorrowedOn >= start);
        }

        if (to is DateOnly end)
        {
            loans = loans.Where(loan => loan.BorrowedOn <= end);
        }

        return await loans
            .GroupBy(loan => loan.BookId)
            .Select(group => new BookLoanCount(group.Key, group.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BorrowerLoanCount>> CountLoansPerBorrowerAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken) =>
        await dbContext.Loans
            .AsNoTracking()
            .Where(loan => loan.BorrowedOn >= from && loan.BorrowedOn <= to)
            .GroupBy(loan => loan.BorrowerId)
            .Select(group => new BorrowerLoanCount(group.Key, group.Count()))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<BookCoBorrowerCount>> CountCoBorrowersAsync(int bookId, CancellationToken cancellationToken)
    {
        var borrowerIds = dbContext.Loans
            .Where(loan => loan.BookId == bookId)
            .Select(loan => loan.BorrowerId);

        return await dbContext.Loans
            .AsNoTracking()
            .Where(loan => loan.BookId != bookId && borrowerIds.Contains(loan.BorrowerId))
            .GroupBy(loan => loan.BookId)
            .Select(group => new BookCoBorrowerCount(group.Key, group.Select(loan => loan.BorrowerId).Distinct().Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClosedLoan>> ClosedLoansOfAsync(int borrowerId, CancellationToken cancellationToken)
    {
        // Whole days are Loan's arithmetic, and EF 10 on Npgsql does not translate DateOnly
        // subtraction, so the closed loans come back and the day count is computed here.
        var loans = await dbContext.Loans
            .AsNoTracking()
            .Where(loan => loan.BorrowerId == borrowerId && loan.ReturnedOn != null)
            .ToListAsync(cancellationToken);

        return [.. loans.Select(loan => new ClosedLoan(loan.BookId, loan.Days))];
    }
}
