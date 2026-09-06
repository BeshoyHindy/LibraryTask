using Library.Shared;
using Mediator;

namespace Insights.Features.GetTopBorrowers;

public sealed record GetTopBorrowersQuery(DateOnly From, DateOnly To, int? Limit)
    : IQuery<Result<IReadOnlyList<RankedBorrower>>>;
