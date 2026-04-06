namespace Common.WikiMedia.Tests;

[TestClass]
public sealed class ApiUrlBuilderTest
{
    [TestMethod]
    [DataRow("http://localhost/wiki/api.php/", "Test_Page", "http://localhost/wiki/api.php/page/Test_Page")]
    [DataRow("https://localhost.com/w/rest.php/v1", "Live:123456", "https://localhost.com/w/rest.php/v1/page/Live:123456")]
    public void GetPageUrl(string baseUrl, string pageName, string expectedOutput)
    {
        var builder = new ApiUrlBuilder(baseUrl);
        var pageUrl = builder.GetPageUrl(pageName).AbsoluteUri;
        Assert.AreEqual(expectedOutput, pageUrl);
    }
}