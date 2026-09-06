namespace Lending.Contracts;

public interface ILoanReader
{
    Task<IReadOnlyList<BookLoanCount>> CountLoansPerBookAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken);

    Task<IReadOnlyList<BorrowerLoanCount>> CountLoansPerBorrowerAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken);

    Task<IReadOnlyList<BookCoBorrowerCount>> CountCoBorrowersAsync(int bookId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ClosedLoan>> ClosedLoansOfAsync(int borrowerId, CancellationToken cancellationToken);
}
