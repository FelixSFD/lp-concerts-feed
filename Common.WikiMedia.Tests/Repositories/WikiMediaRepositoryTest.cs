using System.Net;
using Common.WikiMedia.Repositories;

namespace Common.WikiMedia.Tests.Repositories;

[TestClass]
public class WikiMediaRepositoryTest
{
    [TestMethod]
    public async Task GetWikiPageAsync()
    {
        var mockJson = await File.ReadAllTextAsync("TestData/wiki_page_Live_20240905.json");
        var messageHandler = new MockHttpMessageHandler(mockJson, HttpStatusCode.OK);
        var httpClient = new HttpClient(messageHandler);
        var repo = new WikiMediaRepository(httpClient, "http://localhost/wiki/rest.php/v1");
        
        // run the test
        var wikiPageDto = await repo.GetWikiPageAsync("Live:20240905");
        Assert.IsNotNull(wikiPageDto);
        Assert.AreEqual("Live:20240905", wikiPageDto.Title);
        Assert.AreEqual("wikitext", wikiPageDto.ContentModel);
    }

}