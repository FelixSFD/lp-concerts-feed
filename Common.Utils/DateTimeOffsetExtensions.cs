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
}