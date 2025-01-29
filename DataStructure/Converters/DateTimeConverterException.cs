namespace LPCalendar.DataStructure.Converters;

/// <summary>
/// Exception for converter errors
/// </summary>
/// <param name="message"></param>
/// <param name="innerException"></param>
public class DateTimeConverterException(string message, Exception? innerException = null)
    : Exception(message, innerException);