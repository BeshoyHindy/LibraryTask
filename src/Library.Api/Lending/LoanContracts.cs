namespace Library.Api.Lending;

public sealed record BorrowLoanRequest(int BookId, int BorrowerId, DateOnly BorrowedOn);

public sealed record ReturnLoanRequest(DateOnly ReturnedOn);

public sealed record LoanResponse(int LoanId, int BookId, int BorrowerId, DateOnly BorrowedOn, DateOnly? ReturnedOn);
