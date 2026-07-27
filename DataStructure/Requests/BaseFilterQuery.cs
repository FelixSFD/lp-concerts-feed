namespace LPCalendar.DataStructure.Requests;

public abstract class BaseFilterQuery
{
    /// <summary>
    /// Limit the number of results
    /// </summary>
    public uint Limit { get; set; } = 100;
    
    /// <summary>
    /// Number of results to skip
    /// </summary>
    public uint Skip { get; set; } = 0;
}