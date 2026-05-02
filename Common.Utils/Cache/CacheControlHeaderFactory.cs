using System.Net.Http.Headers;

namespace Common.Utils.Cache;

public static class CacheControlHeaderFactory
{
    private const CacheFlags DefaultCacheFlags = CacheFlags.Public | CacheFlags.MustRevalidate | CacheFlags.UseMaxAgeForServerOnly;
    
    public static CacheControlHeaderValue GetCacheHeaderValueFor(int seconds, CacheFlags flags = DefaultCacheFlags) 
        => GetCacheHeaderValueFor(TimeSpan.FromSeconds(seconds), flags);
    
    public static CacheControlHeaderValue GetCacheHeaderValueFor(TimeSpan timeSpan, CacheFlags flags = DefaultCacheFlags)
    {
        var control = new CacheControlHeaderValue
        {
            MaxAge = flags.HasFlag(CacheFlags.UseMaxAgeForServerOnly) ? TimeSpan.Zero : timeSpan,
            SharedMaxAge = flags.HasFlag(CacheFlags.UseMaxAgeForServerOnly) ? timeSpan : TimeSpan.Zero,
            Public = flags.HasFlag(CacheFlags.Public),
            Private = flags.HasFlag(CacheFlags.Private),
            NoStore = flags.HasFlag(CacheFlags.NoStore),
            MustRevalidate = flags.HasFlag(CacheFlags.MustRevalidate),
            NoCache = flags.HasFlag(CacheFlags.NoCache),
        };
        
        return control;
    }
    
    public static string CacheFor(int seconds, CacheFlags flags = DefaultCacheFlags) => CacheFor(TimeSpan.FromSeconds(seconds), flags);


    public static string CacheFor(TimeSpan timeSpan, CacheFlags flags = DefaultCacheFlags) => GetCacheHeaderValueFor(timeSpan, flags).ToString();


    public static KeyValuePair<string, string> CacheHeaderFieldFor(int seconds, CacheFlags flags = DefaultCacheFlags)
        => CacheHeaderFieldFor(TimeSpan.FromSeconds(seconds), flags);
    
    public static KeyValuePair<string, string> CacheHeaderFieldFor(TimeSpan timeSpan, CacheFlags flags = DefaultCacheFlags)
        => CacheHeaderFieldFor(CacheFor(timeSpan, flags));
    
    public static KeyValuePair<string, string> CacheHeaderFieldFor(string value)
        => new("Cache-Control", value);
}