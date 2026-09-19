using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Common.Database.DataObjects.Types;

/// <summary>
/// Wrapper for Strings that are links to Linkinpedia. This helps EFcore to use certain conventions for the fields
/// </summary>
/// <param name="url">The URL to a Linkinpedia page</param>
public readonly struct LinkinpediaUrl(string url) : IEquatable<LinkinpediaUrl>
{
    private readonly string _url = url;
    
    public static implicit operator string(LinkinpediaUrl url) => url._url;
    public static implicit operator string?(LinkinpediaUrl? url) => url?._url;
    
    public static implicit operator LinkinpediaUrl(string url) => new(url);
    public static implicit operator LinkinpediaUrl?(string? url) => url != null ? new(url) : null;

    public override string ToString() => _url;

    public static bool operator ==(LinkinpediaUrl? left, LinkinpediaUrl? right) => left?._url == right?._url;

    public static bool operator !=(LinkinpediaUrl? left, LinkinpediaUrl? right) => left?._url != right?._url;

    public override bool Equals(object? obj) => obj is LinkinpediaUrl url && _url == url._url;

    public override int GetHashCode() => _url.GetHashCode();

    public bool Equals(LinkinpediaUrl other)
    {
        return _url == other._url;
    }
}


public class LinkinpediaUrlValueConverter() : ValueConverter<LinkinpediaUrl, string>(url => url.ToString(),
    url => new LinkinpediaUrl(url));