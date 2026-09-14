namespace Common.Utils;

public static class DateTimeExtensions
{
    /// <param name="dateTime"></param>
    extension(DateTime dateTime)
    {
        /// <summary>
        /// Converts a <see cref="DateTime"/> to a <see cref="DateTimeOffset"/> keeping the date and time and adding the time zone
        /// </summary>
        /// <param name="timeZoneId">ID of the timezone (like "Europe/Berlin")</param>
        /// <returns>The DateTime with the given timezone information</returns>
        public DateTimeOffset ToDateTimeOffset(string timeZoneId)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return dateTime.ToDateTimeOffset(timeZone);
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> to a <see cref="DateTimeOffset"/> keeping the date and time and adding the time zone
        /// </summary>
        /// <param name="timeZone"></param>
        /// <returns>The DateTime with the given timezone information</returns>
        public DateTimeOffset ToDateTimeOffset(TimeZoneInfo timeZone) =>
            new(dateTime, timeZone.GetUtcOffset(dateTime));
    }
}