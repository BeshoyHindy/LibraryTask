using Library.Warmup;
using Xunit;

namespace Library.UnitTests.Warmup;

public sealed class BookIdsTests
{
    [Fact]
    public void IsPowerOfTwo_Zero_ReturnsFalse()
    {
        var isPowerOfTwo = BookIds.IsPowerOfTwo(0);

        Assert.False(isPowerOfTwo);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    [InlineData(int.MinValue)]
    public void IsPowerOfTwo_Negative_ReturnsFalse(int id)
    {
        var isPowerOfTwo = BookIds.IsPowerOfTwo(id);

        Assert.False(isPowerOfTwo);
    }

    [Fact]
    public void IsPowerOfTwo_One_ReturnsTrue()
    {
        var isPowerOfTwo = BookIds.IsPowerOfTwo(1);

        Assert.True(isPowerOfTwo);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(1024)]
    [InlineData(1 << 30)]
    public void IsPowerOfTwo_PowerOfTwo_ReturnsTrue(int id)
    {
        var isPowerOfTwo = BookIds.IsPowerOfTwo(id);

        Assert.True(isPowerOfTwo);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(12)]
    [InlineData(1000)]
    [InlineData(int.MaxValue)]
    public void IsPowerOfTwo_NotPowerOfTwo_ReturnsFalse(int id)
    {
        var isPowerOfTwo = BookIds.IsPowerOfTwo(id);

        Assert.False(isPowerOfTwo);
    }

    [Fact]
    public void OddIds_ReturnsFiftyOddNumbersFromOneToNinetyNine()
    {
        var ids = BookIds.OddIds().ToArray();

        Assert.Equal(50, ids.Length);
        Assert.Equal(1, ids[0]);
        Assert.Equal(99, ids[^1]);
        Assert.All(ids, id => Assert.Equal(1, id % 2));
    }

    [Fact]
    public void PrintOddIds_WritesOneIdPerLine()
    {
        var writer = new StringWriter();

        BookIds.PrintOddIds(writer);

        var lines = writer.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(50, lines.Length);
        Assert.Equal("1", lines[0]);
        Assert.Equal("3", lines[1]);
        Assert.Equal("99", lines[^1]);
    }
}
