namespace Library.Api.Insights;

public sealed record MostBorrowedBookResponse(int BookId, string Title, string Author, int LoanCount);

public sealed record TopBorrowerResponse(int BorrowerId, string Name, int LoanCount);

public sealed record ReadingPaceResponse(int BorrowerId, double PagesPerDay, int ClosedLoans);

public sealed record AlsoBorrowedBookResponse(int BookId, string Title, string Author, int BorrowerCount);
