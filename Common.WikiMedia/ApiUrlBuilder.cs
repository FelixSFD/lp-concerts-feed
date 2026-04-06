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
}