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

    /// <summary>
    /// List of fields to sort the results by. Only some predefined fields can be used.
    /// To order in descending order, prepend the field name with a dash: "-"
    /// </summary>
    public virtual string[] OrderBy { get; set; } = [];
}