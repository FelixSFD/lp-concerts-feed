using System.Globalization;
using System.Text.RegularExpressions;
using Service.Tours.Importer.DataStructure;

namespace Service.Tours.Importer;

/// <summary>
/// Class to parse the source wikitext from Linkinpedia for tour and concert information
/// </summary>
public partial class TourdataWikitextParser : IWikitextParser
{
    [GeneratedRegex(@"{{Tourdate\b(?:(?<open>{{)|(?<-open>}})|(?!{{|}})[\s\S])*}}(?(open)(?!))", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ExtractTourdateFromSourceRegex { get; }

    [GeneratedRegex(@"^\|\s*(?<key>[^=\r\n]+?)\s*=\s*(?<value>.*)$", RegexOptions.Multiline, "en-US")]
    private static partial Regex ExtractKeyAndValueFromLineRegex { get; }

    /// <summary>
    /// Extract the part of the source that renders the tourdate
    /// </summary>
    /// <param name="pageSource">Source of the whole wiki page</param>
    /// <returns>the tourdate part or null if no tourdate was found</returns>
    public string? ExtractTourdateSource(string pageSource)
    {
        if (string.IsNullOrWhiteSpace(pageSource))
            return null;

        var match = ExtractTourdateFromSourceRegex.Match(pageSource);
        return match.Success ? match.Value : null;
    }

    /// <summary>
    /// Parses the tourdate source and extracts the concert/tourdate information
    /// </summary>
    /// <param name="tourdateSource">Wikitext source of the Tourdate template or the whole wiki page</param>
    /// <returns>Parsed tourdate information or null if no valid tourdate was found</returns>
    public WikiTourdateEntry? GetTourdateInformation(string tourdateSource)
    {
        if (string.IsNullOrWhiteSpace(tourdateSource))
            return null;

        var sourceToParse = ExtractTourdateSource(tourdateSource) ?? tourdateSource;

        var lines = sourceToParse
            .Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(l => l.StartsWith('|'))
            .ToArray();

        if (lines.Length == 0)
            return null;

        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            var match = ExtractKeyAndValueFromLineRegex.Match(line);
            if (match.Success)
            {
                var key = match.Groups["key"].Value.Trim();
                var value = match.Groups["value"].Value.Trim();
                properties[key] = value;
            }
        }

        if (properties.Count == 0)
            return null;

        var year = TryParseUint(GetProperty(properties, "Year"));
        var month = GetProperty(properties, "Month");
        var day = TryParseUint(GetProperty(properties, "Day"));
        var date = TryParseDate(year, month, day);

        var support1 = GetProperty(properties, "Support", "Support1");
        var support2 = GetProperty(properties, "Support2");

        var supportArtists = properties
            .Where(kv => kv.Key.StartsWith("Support", StringComparison.OrdinalIgnoreCase))
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => kv.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct()
            .ToArray();

        var entry = new WikiTourdateEntry
        {
            ShowType = GetProperty(properties, "ShowType", "Show Type"),
            Artist = GetProperty(properties, "Artist"),
            Event = GetProperty(properties, "Event"),
            LastShow = GetProperty(properties, "Last show", "LastShow", "Last Show"),
            NextShow = GetProperty(properties, "Next show", "NextShow", "Next Show"),
            Year = year,
            Month = month,
            Day = day,
            Date = date,
            Country = GetProperty(properties, "Country"),
            City = GetProperty(properties, "City"),
            CityLocal = GetProperty(properties, "City local", "CityLocal", "City Local"),
            Venue = GetProperty(properties, "Venue"),
            VenueId = GetProperty(properties, "VenueID", "VenueId", "Venue ID"),
            VenueType = GetProperty(properties, "Venue Type", "VenueType"),
            VenueWebsite = GetProperty(properties, "Venue Website", "VenueWebsite"),
            Tour = GetProperty(properties, "Tour"),
            TourLeg = GetProperty(properties, "Tour leg", "TourLeg", "Tour Leg"),
            Stage = GetProperty(properties, "Stage"),
            OtherArtists = GetProperty(properties, "Other artists", "OtherArtists", "Other Artist"),
            Support = support1,
            Support2 = support2,
            SupportArtists = supportArtists,
            Setlist = GetProperty(properties, "Setlist"),
            RawProperties = properties
        };

        return entry;
    }

    /// <summary>
    /// Parses the tourdate source and extracts the concert/tourdate information
    /// </summary>
    /// <param name="tourdateSource">Wikitext source of the Tourdate template or the whole wiki page</param>
    /// <returns>Parsed tourdate information or null if no valid tourdate was found</returns>
    public WikiTourdateEntry? GetTourdate(string tourdateSource) => GetTourdateInformation(tourdateSource);

    private static string? GetProperty(Dictionary<string, string> dict, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (dict.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val))
            {
                return val;
            }
        }

        return null;
    }

    private static uint? TryParseUint(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return uint.TryParse(value, out var result) ? result : null;
    }

    private static DateOnly? TryParseDate(uint? year, string? monthStr, uint? day)
    {
        if (year is null or 0 || string.IsNullOrWhiteSpace(monthStr) || day is null or 0)
            return null;

        int month = 0;
        if (int.TryParse(monthStr, out var m) && m is >= 1 and <= 12)
        {
            month = m;
        }
        else
        {
            if (DateTime.TryParseExact(monthStr.Trim(), ["MMMM", "MMM"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedMonth))
            {
                month = parsedMonth.Month;
            }
            else if (string.Equals(monthStr.Trim(), "Sept", StringComparison.OrdinalIgnoreCase))
            {
                month = 9;
            }
        }

        if (month is >= 1 and <= 12 && day is >= 1 and <= 31)
        {
            try
            {
                return new DateOnly((int)year.Value, month, (int)day.Value);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }
}
