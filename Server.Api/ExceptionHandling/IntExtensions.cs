namespace Server.Api.ExceptionHandling;

/// <summary>
/// Extension to handle uint/int conversions with exception handling
/// </summary>
public static class IntExtensions
{
    /// <summary>
    /// Converts the <see cref="int"/> to <see cref="uint"/> or throws an exception if the values is less than 0
    /// </summary>
    /// <param name="value"></param>
    /// <returns>unsigned integer</returns>
    /// <exception cref="ArgumentException">if the value is less than 0</exception>
    public static uint ConvertToUnsigned(this int value)
    {
        try
        {
            return Convert.ToUInt32(value);
        } catch (OverflowException e)
        {
            throw new ArgumentException($"Cannot convert integer to unsigned integer, because the value {value} is less than zero.", e);
        }
    }
}