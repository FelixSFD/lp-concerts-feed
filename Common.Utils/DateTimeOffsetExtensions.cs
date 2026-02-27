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
        var millisecondCorrection = 1000 - dateTimeOffset.Millisecond;
        return millisecondCorrection == 1000 ? dateTimeOffset : dateTimeOffset.AddMilliseconds(millisecondCorrection);
    }
}