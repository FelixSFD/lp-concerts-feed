namespace Common.Utils.Cache;

public static class CacheConstants
{
    /// <summary>
    /// 2 minutes
    /// </summary>
    public const int ShortExpirationInSeconds = 120;
    
    /// <summary>
    /// 1 hour
    /// </summary>
    public const int MediumExpirationInSeconds = 3_600;
    
    /// <summary>
    /// 4 hours
    /// </summary>
    public const int LongExpirationInSeconds = 14_400;
    
    /// <summary>
    /// 12 hours
    /// </summary>
    public const int VeryLongExpirationInSeconds = 43_200;
    
    /// <summary>
    /// 24 hours (that's the limit set in our CloudFront policy)
    /// </summary>
    public const int MaximumExpirationInSeconds = 86_400;
    
    public const int DefaultExpirationInSeconds = ShortExpirationInSeconds;
}