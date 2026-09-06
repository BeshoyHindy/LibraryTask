using Insights.Features.GetMostBorrowedBooks;
using Xunit;

namespace Library.UnitTests.Insights;

public sealed class GetMostBorrowedBooksQueryValidatorTests
{
    private static readonly DateOnly From = new(2025, 1, 1);
    private static readonly DateOnly To = new(2025, 6, 30);

    private readonly GetMostBorrowedBooksQueryValidator validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void Validate_ALimitTheRuleAllows_IsValid(int? limit)
    {
        var result = validator.Validate(new GetMostBorrowedBooksQuery(From, To, limit));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(101)]
    [InlineData(-1)]
    public void Validate_ALimitOutsideTheRule_FailsOnLimit(int limit)
    {
        var result = validator.Validate(new GetMostBorrowedBooksQuery(From, To, limit));

        Assert.Equal("Limit", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Validate_NoRange_IsValid()
    {
        var result = validator.Validate(new GetMostBorrowedBooksQuery(From: null, To: null, Limit: null));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Validate_HalfARange_IsValid(bool hasFrom, bool hasTo)
    {
        var result = validator.Validate(new GetMostBorrowedBooksQuery(hasFrom ? From : null, hasTo ? To : null, Limit: null));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ARangeOfOneDay_IsValid()
    {
        var result = validator.Validate(new GetMostBorrowedBooksQuery(From, From, Limit: null));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_AnInvertedRange_FailsOnTo()
    {
        var result = validator.Validate(new GetMostBorrowedBooksQuery(To, From, Limit: null));

        Assert.Equal("To", Assert.Single(result.Errors).PropertyName);
    }
}
