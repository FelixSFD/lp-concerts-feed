using System.Net.Http.Headers;

namespace Common.Utils.Cache;

public static class CacheControlHeaderFactory
{
    public static string CacheFor(int seconds, CacheFlags flags = CacheFlags.Public) => CacheFor(TimeSpan.FromSeconds(seconds), flags);


    public static string CacheFor(TimeSpan timeSpan, CacheFlags flags = CacheFlags.Public)
    {
        var control = new CacheControlHeaderValue
        {
            MaxAge = timeSpan,
            Public =  flags.HasFlag(CacheFlags.Public),
            Private = flags.HasFlag(CacheFlags.Private),
        };
        
        return control.ToString();
    }
    
    
    public static KeyValuePair<string, string> CacheHeaderFieldFor(int seconds, CacheFlags flags = CacheFlags.Public)
        => CacheHeaderFieldFor(TimeSpan.FromSeconds(seconds), flags);
    
    public static KeyValuePair<string, string> CacheHeaderFieldFor(TimeSpan timeSpan, CacheFlags flags = CacheFlags.Public)
        => new("Cache-Control", CacheFor(timeSpan, flags));
}