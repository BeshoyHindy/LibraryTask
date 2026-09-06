using Library.Shared;
using Mediator;

namespace Insights.Features.GetReadingPace;

// No validator: a borrower id the library does not hold is a NotFound, whatever its value.
public sealed record GetReadingPaceQuery(int BorrowerId) : IQuery<Result<ReadingPace>>;
