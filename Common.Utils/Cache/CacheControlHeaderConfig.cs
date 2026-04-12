using System.Net.Http.Headers;

namespace Common.Utils.Cache;

public static class CacheControlHeaderConfig
{
    /// <summary>
    /// Default cache settings
    /// </summary>
    public static readonly CacheControlHeaderValue Default = CacheControlHeaderFactory.GetCacheHeaderValueFor(CacheExpiration.Default);
}