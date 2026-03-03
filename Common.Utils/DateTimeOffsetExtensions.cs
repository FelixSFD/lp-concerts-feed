namespace Common.Utils;

public static class DateTimeOffsetExtensions
{
    /// <summary>
    /// Rounds UP the date to the next full second
    /// </summary>
    /// <param name="dateTimeOffset"></param>
    /// <returns></returns>
    public static DateTimeOffset RoundingUpToSecond(this DateTimeOffset dateTimeOffset)
    {
        var result = dateTimeOffset;
        
        var microsecondCorrection = 1_000 - result.Microsecond;
        if (microsecondCorrection != 1_000)
        {
            result = result.AddMicroseconds(microsecondCorrection);
        }
        
        var millisecondCorrection = 1_000 - result.Millisecond;
        if (millisecondCorrection != 1_000)
        {
            result = result.AddMilliseconds(millisecondCorrection);
        }
        
        return result;
    }


    /// <summary>
    /// Returns the maximum value of the two passed parameters
    /// </summary>
    /// <param name="dateA"></param>
    /// <param name="dateB"></param>
    /// <returns></returns>
    public static DateTimeOffset? Max(DateTimeOffset? dateA, DateTimeOffset? dateB)
    {
        if (dateA == dateB)
        {
            return dateA;
        }
        
        var safeA = dateA ?? DateTimeOffset.MinValue;
        var safeB = dateB ?? DateTimeOffset.MinValue;
        return safeA > safeB ? safeA : safeB;
    }
}