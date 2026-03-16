namespace Common.Utils.Tests;

public class StringUtilsTest
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(" ", " ")]
    [InlineData("test", "test")]
    [InlineData("Hello, World!", "Hello, World!")]
    public void NullIfEmpty(string? input, string? expected)
    {
        var result = StringUtils.NullIfEmpty(input);
        Assert.Equal(expected, result);
    }
}