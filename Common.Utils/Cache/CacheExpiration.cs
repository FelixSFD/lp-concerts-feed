namespace Common.Utils.Cache;

public static class CacheExpiration
{
    /// <summary>
    /// 2 minutes
    /// </summary>
    public const int Short = 120;
    
    /// <summary>
    /// 1 hour
    /// </summary>
    public const int Medium = 3_600;
    
    /// <summary>
    /// 4 hours
    /// </summary>
    public const int Long = 14_400;
    
    /// <summary>
    /// 12 hours
    /// </summary>
    public const int VeryLong = 43_200;
    
    /// <summary>
    /// 24 hours (that's the limit set in our CloudFront policy)
    /// </summary>
    public const int Maximum = 86_400;
    
    public const int Default = Short;
}