using System.Net.Http.Headers;

namespace Common.Utils.Cache;

public static class CacheControlHeaderConfig
{
    /// <summary>
    /// Default cache settings
    /// </summary>
    public static readonly CacheControlHeaderValue Default = CacheControlHeaderFactory.GetCacheHeaderValueFor(CacheExpiration.Default);
    
    public static readonly CacheControlHeaderValue Long = CacheControlHeaderFactory.GetCacheHeaderValueFor(CacheExpiration.Long);
    
    /// <summary>
    /// No caching
    /// </summary>
    public static readonly CacheControlHeaderValue None = CacheControlHeaderFactory.GetCacheHeaderValueFor(TimeSpan.Zero, CacheFlags.None);
}