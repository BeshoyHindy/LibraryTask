using Lending.Features.BorrowBook;
using Xunit;

namespace Library.UnitTests.Lending;

public sealed class BorrowBookCommandValidatorTests
{
    private static readonly DateOnly BorrowedOn = new(2026, 3, 1);

    private readonly BorrowBookCommandValidator validator = new();

    [Fact]
    public void Validate_PositiveIds_IsValid()
    {
        var result = validator.Validate(Command(bookId: 1, borrowerId: 1));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ABookIdThatIsNotPositive_FailsOnBookId(int bookId)
    {
        var result = validator.Validate(Command(bookId, borrowerId: 1));

        Assert.Equal("BookId", Assert.Single(result.Errors).PropertyName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ABorrowerIdThatIsNotPositive_FailsOnBorrowerId(int borrowerId)
    {
        var result = validator.Validate(Command(bookId: 1, borrowerId));

        Assert.Equal("BorrowerId", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Validate_BothIdsMissing_FailsOnBoth()
    {
        var result = validator.Validate(Command(bookId: 0, borrowerId: 0));

        Assert.Equal(["BookId", "BorrowerId"], result.Errors.Select(failure => failure.PropertyName));
    }

    private static BorrowBookCommand Command(int bookId, int borrowerId) => new(bookId, borrowerId, BorrowedOn);
}
