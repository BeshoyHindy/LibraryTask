using Insights.Features.GetAlsoBorrowedBooks;
using Xunit;

namespace Library.UnitTests.Insights;

public sealed class GetAlsoBorrowedBooksQueryValidatorTests
{
    private readonly GetAlsoBorrowedBooksQueryValidator validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void Validate_ALimitTheRuleAllows_IsValid(int? limit)
    {
        var result = validator.Validate(new GetAlsoBorrowedBooksQuery(BookId: 4, limit));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(101)]
    [InlineData(-1)]
    public void Validate_ALimitOutsideTheRule_FailsOnLimit(int limit)
    {
        var result = validator.Validate(new GetAlsoBorrowedBooksQuery(BookId: 4, limit));

        Assert.Equal("Limit", Assert.Single(result.Errors).PropertyName);
    }
}
