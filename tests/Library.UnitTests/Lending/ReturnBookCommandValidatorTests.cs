using Lending.Features.ReturnBook;
using Xunit;

namespace Library.UnitTests.Lending;

public sealed class ReturnBookCommandValidatorTests
{
    private static readonly DateOnly ReturnedOn = new(2026, 3, 5);

    private readonly ReturnBookCommandValidator validator = new();

    [Fact]
    public void Validate_APositiveLoanId_IsValid()
    {
        var result = validator.Validate(new ReturnBookCommand(1, ReturnedOn));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ALoanIdThatIsNotPositive_FailsOnLoanId(int loanId)
    {
        var result = validator.Validate(new ReturnBookCommand(loanId, ReturnedOn));

        Assert.Equal("LoanId", Assert.Single(result.Errors).PropertyName);
    }
}
