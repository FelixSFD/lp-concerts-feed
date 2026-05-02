using Common.Utils.Cache;

namespace Common.Utils.Tests.Cache;


public class CacheControlHeaderFactoryTest
{
    [Theory]
    [InlineData(120, CacheFlags.None, "max-age=120")]
    [InlineData(0, CacheFlags.None, "max-age=0")]
    [InlineData(1234, CacheFlags.Public, "public, max-age=1234")]
    [InlineData(59, CacheFlags.Public | CacheFlags.Private, "public, max-age=59, private")]
    [InlineData(59, CacheFlags.Private, "max-age=59, private")]
    [InlineData(14400, CacheFlags.Public | CacheFlags.MustRevalidate, "public, must-revalidate, max-age=14400")]
    [InlineData(120, CacheFlags.UseMaxAgeForServerOnly, "max-age=0, s-maxage=120")]
    public void WithFlags(int seconds, CacheFlags flags, string expectedHeaderValue)
    {
        var generatedValue = CacheControlHeaderFactory.CacheFor(seconds, flags);
        Assert.Equal(expectedHeaderValue, generatedValue);
    }
    
    [Theory]
    [InlineData(120, "public, must-revalidate, max-age=0, s-maxage=120")]
    [InlineData(0, "public, must-revalidate, max-age=0, s-maxage=0")]
    [InlineData(1234, "public, must-revalidate, max-age=0, s-maxage=1234")]
    public void WithFlags_Default(int seconds, string expectedHeaderValue)
    {
        var generatedValue = CacheControlHeaderFactory.CacheFor(seconds);
        Assert.Equal(expectedHeaderValue, generatedValue);
    }
}