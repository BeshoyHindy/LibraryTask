namespace Insights.Features.GetTopBorrowers;

/// <summary>A borrower in the ranking, with the loans they took inside the range.</summary>
public sealed record RankedBorrower(int BorrowerId, string Name, int LoanCount);
