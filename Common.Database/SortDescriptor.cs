namespace Common.Database;

/// <summary>
/// Describes one of the fields to sort by in a query.
/// </summary>
/// <param name="Property">Name of the property as defined in the repository of that query</param>
/// <param name="Descending">true, if the field should be sorted in descending order</param>
public record SortDescriptor(
    string Property,
    bool Descending = false)
{
    public override string ToString()
    {
        return Descending ? $"-{Property}" : Property;
    }

    /// <summary>
    /// Creates a new <see cref="SortDescriptor"/> from a string. The string must be the property name. To order descending, prepend the property name with a dash: "-"
    /// </summary>
    /// <param name="str"></param>
    /// <returns>new instance of the sort descriptor</returns>
    public static SortDescriptor FromString(string str)
    {
        var isDesc = str.StartsWith("-");
        return isDesc ? new SortDescriptor(str[1..], isDesc) : new SortDescriptor(str);
    }
}