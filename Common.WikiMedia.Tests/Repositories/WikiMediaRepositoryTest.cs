using System.Net;
using Common.WikiMedia.DTOs;
using Common.WikiMedia.Repositories;
using Common.WikiMedia.Tests.DTOs;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Common.WikiMedia.Tests.Repositories;

[TestClass]
public class WikiMediaRepositoryTest
{
    private readonly ILogger<WikiMediaRepository> _logger = Substitute.For<ILogger<WikiMediaRepository>>();

    [TestMethod]
    public async Task GetWikiPageAsync()
    {
        var mockJson = await File.ReadAllTextAsync("TestData/wiki_page_Live_20240905.json", TestContext.CancellationToken);
        var messageHandler = new MockHttpMessageHandler(mockJson, HttpStatusCode.OK);
        var httpClient = new HttpClient(messageHandler);
        var repo = new WikiMediaRepository(httpClient, "http://localhost/wiki/rest.php/v1", _logger);
        
        // run the test
        var wikiPageDto = await repo.GetWikiPageAsync("Live:20240905");
        Assert.IsNotNull(wikiPageDto);
        Assert.AreEqual("Live:20240905", wikiPageDto.Title);
        Assert.AreEqual("wikitext", wikiPageDto.ContentModel);
    }

    [TestMethod]
    public async Task RunCargoQueryAsync()
    {
        var mockJson = await File.ReadAllTextAsync("TestData/wiki_cargo_shows_lp_10.json", TestContext.CancellationToken);
        var messageHandler = new MockHttpMessageHandler(mockJson, HttpStatusCode.OK);
        var httpClient = new HttpClient(messageHandler);
        var repo = new WikiMediaRepository(httpClient, "https://linkinpedia.com/w", _logger);

        string[] tables = ["Shows"];
        string[] fields = ["Artist", "ShowPage", "Date", "ShowType", "Country", "State", "Province", "UKCountry", "City", "Venue", "Tour", "TourLeg", "Date__precision"];
        CargoQueryWhereClause[] where = [
            new()
            {
                FieldName = "Artist",
                Value = "Linkin Park",
                Comparison = CargoQueryWhereClause.Operation.IsEqual,
            }
        ];
        string[] orderBy = ["Date"];

        var results = new List<CargoQueryResponseItemDto<CargoShowTestDto>>();
        await foreach (var item in repo.RunCargoQueryAsync<CargoShowTestDto>(tables, fields, where, orderBy, cancellationToken: TestContext.CancellationToken))
        {
            results.Add(item);
        }

        Assert.HasCount(10, results);
        Assert.AreEqual(1, messageHandler.NumberOfCalls);

        var first = results[0].Value;
        Assert.IsNotNull(first);
        Assert.AreEqual("Linkin Park", first.Artist);
        Assert.AreEqual("Live:20000525", first.ShowPage);
        Assert.AreEqual("2000-05-25", first.Date);
        Assert.AreEqual("concert", first.ShowType);
        Assert.AreEqual("United States", first.Country);
        Assert.AreEqual("California", first.State);
        Assert.AreEqual(string.Empty, first.Province);
        Assert.AreEqual(string.Empty, first.UKCountry);
        Assert.AreEqual("West Hollywood, CA", first.City);
        Assert.AreEqual("Coconut Teaszer", first.Venue);
        Assert.AreEqual(string.Empty, first.Tour);
        Assert.AreEqual(string.Empty, first.TourLeg);
        Assert.AreEqual("1", first.DatePrecision);

        var sixth = results[5].Value;
        Assert.IsNotNull(sixth);
        Assert.AreEqual("Live:20000722", sixth.ShowPage);
        Assert.AreEqual("An Education In Rebellion Tour 2000", sixth.Tour);

        var last = results[9].Value;
        Assert.IsNotNull(last);
        Assert.AreEqual("Live:20000728", last.ShowPage);
        Assert.AreEqual("Lancaster, PA", last.City);
        Assert.AreEqual("The Chameleon", last.Venue);
    }

    public TestContext TestContext { get; set; }
}