namespace Common.Utils;

public static class StringUtils
{
    /// <summary>
    /// Returns null if the provided string is empty
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string? NullIfEmpty(string? s)
    {
        return string.IsNullOrEmpty(s) ? null : s;
    }
}