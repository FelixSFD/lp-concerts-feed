namespace Common.Utils.Tests;

public class DateTimeOffsetExtensionsTest
{
    [Theory]
    [InlineData("2026-02-26T12:34:56.091", "2026-02-26T12:34:57.000000")]
    [InlineData("2026-02-26T12:34:56.091123", "2026-02-26T12:34:57.000000")]
    [InlineData("2026-02-26T12:34:56.000", "2026-02-26T12:34:56.000000")]
    [InlineData("2026-02-26T12:34:56.000429", "2026-02-26T12:34:57.000000")]
    [InlineData("2000-12-31T00:13:37.999", "2000-12-31T00:13:38.000000")]
    [InlineData("2000-12-31T00:13:37.999999", "2000-12-31T00:13:38.000000")]
    public void RoundMicroseconds(string input, string expectedOutput)
    {
        var inputDate = DateTimeOffset.Parse(input);
        var expectedOutputDate = DateTimeOffset.Parse(expectedOutput);

        inputDate = inputDate.RoundingUpToSecond();
        Assert.Equal(expectedOutputDate, inputDate);
        Assert.Equal(0, inputDate.Millisecond);
        Assert.Equal(0, inputDate.Microsecond);
    }
    
    
    [Theory]
    [InlineData("2026-02-26T12:34:56.091", "2026-02-26T12:34:56.091", "2026-02-26T12:34:56.091")]
    [InlineData("2026-02-26T12:34:56.191", "2026-02-26T12:34:56.091", "2026-02-26T12:34:56.191")]
    [InlineData("2026-02-26T12:34:56.191", "2026-02-26T12:34:56.791", "2026-02-26T12:34:56.791")]
    [InlineData("2026-02-01T12:34:56.000", "2026-03-26T12:00:00.000", "2026-03-26T12:00:00.000")]
    public void Max(string inputA, string inputB, string expectedOutput)
    {
        var inputDateA = DateTimeOffset.Parse(inputA);
        var inputDateB = DateTimeOffset.Parse(inputB);
        var expectedOutputDate = DateTimeOffset.Parse(expectedOutput);

        var max = DateTimeOffsetExtensions.Max(inputDateA, inputDateB);
        Assert.Equal(expectedOutputDate, max);
    }
    
    
    [Fact]
    public void Max_Null()
    {
        var inputDate = new DateTimeOffset(2024, 9, 5, 15, 0, 0, TimeSpan.Zero);

        DateTimeOffset? max = DateTimeOffsetExtensions.Max(inputDate, null);
        Assert.Equal(inputDate, max);
        
        max = DateTimeOffsetExtensions.Max(null, inputDate);
        Assert.Equal(inputDate, max);
        
        max = DateTimeOffsetExtensions.Max(null, null);
        Assert.Null(max);
    }
}