namespace LPCalendar.DataStructure;

public static class LocationStringBuilder
{
    /// <summary>
    /// Returns a single readable string for the location
    /// </summary>
    /// <param name="venue"></param>
    /// <param name="city"></param>
    /// <param name="state"></param>
    /// <param name="country"></param>
    /// <returns></returns>
    public static string GetLocationString(string? venue = null, string? city = null, string? state = null, string? country = null)
    {
        string?[] parts = [venue, city, state, country];
        return string.Join(", ", parts.Where(part => !string.IsNullOrEmpty(part)));
    }
}