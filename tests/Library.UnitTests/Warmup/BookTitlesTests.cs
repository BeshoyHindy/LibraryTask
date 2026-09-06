using Library.Warmup;
using Xunit;

namespace Library.UnitTests.Warmup;

public sealed class BookTitlesTests
{
    private const string BookEmoji = "📚";
    private const string OpenBookEmoji = "📖";

    [Fact]
    public void Reverse_MobyDick_ReturnsKcidYbom()
    {
        var reversed = BookTitles.Reverse("Moby Dick");

        Assert.Equal("kciD yboM", reversed);
    }

    [Fact]
    public void Reverse_EvenLength_ReturnsReversed()
    {
        var reversed = BookTitles.Reverse("Dune");

        Assert.Equal("enuD", reversed);
    }

    [Fact]
    public void Reverse_Empty_ReturnsEmpty()
    {
        var reversed = BookTitles.Reverse(string.Empty);

        Assert.Equal(string.Empty, reversed);
    }

    [Fact]
    public void Reverse_SingleCharacter_ReturnsSame()
    {
        var reversed = BookTitles.Reverse("A");

        Assert.Equal("A", reversed);
    }

    [Fact]
    public void Reverse_SurrogatePair_KeepsPairIntact()
    {
        var reversed = BookTitles.Reverse("Moby " + BookEmoji);

        Assert.Equal(BookEmoji + " yboM", reversed);
    }

    [Fact]
    public void Reverse_AdjacentSurrogatePairs_KeepsPairsIntact()
    {
        var reversed = BookTitles.Reverse(BookEmoji + OpenBookEmoji);

        Assert.Equal(OpenBookEmoji + BookEmoji, reversed);
    }

    [Fact]
    public void Reverse_UnpairedHighSurrogate_DoesNotFormANewPair()
    {
        const string unpairedHighSurrogate = "\uD83D";

        var reversed = BookTitles.Reverse(unpairedHighSurrogate + BookEmoji);

        Assert.Equal(BookEmoji + unpairedHighSurrogate, reversed);
    }

    [Fact]
    public void Reverse_UnpairedLowSurrogate_ReturnsReversed()
    {
        const string unpairedLowSurrogate = "\uDCDA";

        var reversed = BookTitles.Reverse("A" + unpairedLowSurrogate);

        Assert.Equal(unpairedLowSurrogate + "A", reversed);
    }

    [Fact]
    public void Reverse_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => BookTitles.Reverse(null!));
    }

    [Fact]
    public void Replicate_ReadThreeTimes_ReturnsReadReadRead()
    {
        var replicated = BookTitles.Replicate("Read", 3);

        Assert.Equal("ReadReadRead", replicated);
    }

    [Fact]
    public void Replicate_ZeroCount_ReturnsEmpty()
    {
        var replicated = BookTitles.Replicate("Read", 0);

        Assert.Equal(string.Empty, replicated);
    }

    [Fact]
    public void Replicate_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BookTitles.Replicate("Read", -1));
    }

    [Fact]
    public void Replicate_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => BookTitles.Replicate(null!, 3));
    }
}
