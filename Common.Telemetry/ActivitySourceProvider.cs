using System.Diagnostics;

namespace Common.Utils.Diagnostics;

/// <summary>
/// Provides a centralized definition for the ActivitySource used for performance diagnostics.
/// </summary>
public static class ActivitySourceProvider
{
    /// <summary>
    /// The name of the ActivitySource used for performance diagnostics.
    /// </summary>
    public const string ActivitySourceName = "LPshows.Performance";
    
    /// <summary>
    /// The default ActivitySource instance used for performance diagnostics.
    /// </summary>
    public static ActivitySource Default { get; } = new(ActivitySourceName);
}