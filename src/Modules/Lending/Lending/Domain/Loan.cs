using Library.Shared;

namespace Lending.Domain;

public sealed class Loan
{
    private Loan()
    {
    }

    public int Id { get; private set; }

    public int BookId { get; private set; }

    public int BorrowerId { get; private set; }

    public DateOnly BorrowedOn { get; private set; }

    public DateOnly? ReturnedOn { get; private set; }

    public bool IsOpen => ReturnedOn is null;

    public int Days
    {
        get
        {
            if (ReturnedOn is null)
            {
                throw new InvalidOperationException("An open loan has no day count.");
            }

            return Math.Max(1, ReturnedOn.Value.DayNumber - BorrowedOn.DayNumber);
        }
    }

    public static Loan Open(int bookId, int borrowerId, DateOnly borrowedOn) =>
        new() { BookId = bookId, BorrowerId = borrowerId, BorrowedOn = borrowedOn };

    public Result Return(DateOnly returnedOn)
    {
        if (!IsOpen)
        {
            return Error.Conflict($"Loan {Id} is already closed.");
        }

        if (returnedOn < BorrowedOn)
        {
            return Error.Validation(
            [
                new FieldError("returnedOn", $"must not be before borrowedOn {BorrowedOn:yyyy-MM-dd}."),
            ]);
        }

        ReturnedOn = returnedOn;

        return Result.Success();
    }
}
