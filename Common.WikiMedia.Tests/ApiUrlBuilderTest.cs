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

    [TestMethod]
    public void GetCargoQueryUrl_LpShowsSample()
    {
        string[] tables = ["Shows"];
        string[] fields = ["Artist", "ShowPage", "Date", "ShowType", "Country", "State", "Province", "UKCountry", "City", "Venue", "Tour", "TourLeg"];
        CargoQueryWhereClause[] where = [
            new()
            {
                FieldName = "Artist",
                Value = "Linkin Park",
                Comparison = CargoQueryWhereClause.Operation.IsEqual,
            }
        ];
        string[] orderBy = ["Date"];

        const string expectedUrl = "https://linkinpedia.com/w/api.php?action=cargoquery&format=json&limit=200&tables=Shows&fields=Artist%2C+ShowPage%2C+Date%2C+ShowType%2C+Country%2C+State%2C+Province%2C+UKCountry%2C+City%2C+Venue%2C+Tour%2C+TourLeg&where=Artist+%3D+%22Linkin+Park%22&group_by=&order_by=Date&offset=400&formatversion=2";
        var builder = new ApiUrlBuilder("https://linkinpedia.com/w");
        var url = builder.GetCargoQueryUrl(tables, fields, where, orderBy, 200, 400).AbsoluteUri;
        Assert.AreEqual(expectedUrl, url);
    }
}