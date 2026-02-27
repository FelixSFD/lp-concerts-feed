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
}