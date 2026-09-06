using Lending.Domain;
using Library.Shared;
using Xunit;

namespace Library.UnitTests.Lending;

public sealed class LoanTests
{
    private static readonly DateOnly BorrowedOn = new(2026, 3, 1);

    [Fact]
    public void Open_NewLoan_IsOpenWithNoReturnDate()
    {
        var loan = OpenLoan();

        Assert.True(loan.IsOpen);
        Assert.Null(loan.ReturnedOn);
    }

    [Fact]
    public void Open_NewLoan_CarriesTheBookBorrowerAndDate()
    {
        var loan = Loan.Open(bookId: 7, borrowerId: 3, BorrowedOn);

        Assert.Equal(7, loan.BookId);
        Assert.Equal(3, loan.BorrowerId);
        Assert.Equal(BorrowedOn, loan.BorrowedOn);
    }

    [Fact]
    public void Return_OpenLoan_ClosesItOnThatDate()
    {
        var loan = OpenLoan();

        var result = loan.Return(BorrowedOn.AddDays(4));

        Assert.True(result.IsSuccess);
        Assert.False(loan.IsOpen);
        Assert.Equal(BorrowedOn.AddDays(4), loan.ReturnedOn);
    }

    [Fact]
    public void Return_SameDayAsBorrowed_Succeeds()
    {
        var loan = OpenLoan();

        var result = loan.Return(BorrowedOn);

        Assert.True(result.IsSuccess);
        Assert.Equal(BorrowedOn, loan.ReturnedOn);
    }

    [Fact]
    public void Return_AlreadyClosed_IsAConflict()
    {
        var loan = OpenLoan();
        loan.Return(BorrowedOn.AddDays(4));

        var result = loan.Return(BorrowedOn.AddDays(5));

        Assert.Equal(ErrorKind.Conflict, result.Error?.Kind);
        Assert.Equal(BorrowedOn.AddDays(4), loan.ReturnedOn);
    }

    [Fact]
    public void Return_BeforeBorrowedOn_IsAValidationErrorOnReturnedOn()
    {
        var loan = OpenLoan();

        var result = loan.Return(BorrowedOn.AddDays(-1));

        Assert.Equal(ErrorKind.Validation, result.Error?.Kind);
        Assert.Equal("returnedOn", Assert.Single(result.Error?.Fields ?? []).Field);
    }

    [Fact]
    public void Return_BeforeBorrowedOn_LeavesTheLoanOpen()
    {
        var loan = OpenLoan();

        loan.Return(BorrowedOn.AddDays(-1));

        Assert.True(loan.IsOpen);
    }

    [Fact]
    public void Days_SameDayReturn_IsOne()
    {
        var loan = OpenLoan();
        loan.Return(BorrowedOn);

        Assert.Equal(1, loan.Days);
    }

    [Fact]
    public void Days_TenDaysLater_IsTen()
    {
        var loan = OpenLoan();
        loan.Return(BorrowedOn.AddDays(10));

        Assert.Equal(10, loan.Days);
    }

    [Fact]
    public void Days_OpenLoan_ThrowsInvalidOperationException()
    {
        var loan = OpenLoan();

        Assert.Throws<InvalidOperationException>(() => loan.Days);
    }

    private static Loan OpenLoan() => Loan.Open(bookId: 1, borrowerId: 1, BorrowedOn);
}
