namespace Lending.Contracts;

public sealed record LoanDto(int Id, int BookId, int BorrowerId, DateOnly BorrowedOn, DateOnly? ReturnedOn);
