using System.Web;

namespace Common.WikiMedia;

/// <summary>
/// Builds the URLs for the Wiki API
/// </summary>
internal class ApiUrlBuilder
{
    private readonly string _baseUrl;

    /// <summary>
    /// Builds the URLs for the Wiki API
    /// </summary>
    public ApiUrlBuilder(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
    }

    /// <summary>
    /// Returns the URL for /rest.php/v1/page/
    /// </summary>
    /// <param name="wikiPageId"></param>
    /// <returns></returns>
    internal Uri GetPageUrl(string wikiPageId)
    {
        return new Uri($"{_baseUrl}/page/{wikiPageId}");
    }

    internal Uri GetCargoQueryUrl(string[] tables, string[] fields, CargoQueryWhereClause[] where, string[] orderBy,
        int limit = 100, int offset = 0)
    {
        // Example: https://linkinpedia.com/w/api.php?action=cargoquery&format=json&limit=200&tables=Shows&fields=Artist,ShowPage,Date,ShowType,Country,State,Province,UKCountry,City,Venue,Tour,TourLeg&where=Artist%3D%22LinkinPark%22&group_by=&order_by=Date&offset=0&formatversion=2
        var tablesString = string.Join(',', tables);
        var fieldsString = string.Join(", ", fields);
        var whereString = string.Join(", ", where.Select(w => w.ToString()));
        var orderByString = string.Join(", ", orderBy);

        var encodedUrlString =
            $"{_baseUrl}/api.php?action=cargoquery&format=json&limit={limit}&tables={HttpUtility.UrlEncode(tablesString)}&fields={HttpUtility.UrlEncode(fieldsString)}&where={HttpUtility.UrlEncode(whereString)}&group_by=&order_by={HttpUtility.UrlEncode(orderByString)}&offset={offset}&formatversion=2";
        // turn all percent-encodings into uppercase
        encodedUrlString = NormalizePercentEncoding(encodedUrlString);
        return new Uri(encodedUrlString);
    }
    
    
    private static string NormalizePercentEncoding(string value)
    {
        return string.Create(value.Length, value, static (destination, source) =>
        {
            source.AsSpan().CopyTo(destination);

            for (var i = 0; i < destination.Length - 2; i++)
            {
                if (destination[i] == '%' &&
                    Uri.IsHexDigit(destination[i + 1]) &&
                    Uri.IsHexDigit(destination[i + 2]))
                {
                    destination[i + 1] = char.ToUpperInvariant(destination[i + 1]);
                    destination[i + 2] = char.ToUpperInvariant(destination[i + 2]);
                    i += 2;
                }
            }
        });
    }
}