namespace Common.Database;

/// <summary>
/// Describes one of the fields to sort by in a query.
/// </summary>
/// <param name="Property">Name of the property as defined in the repository of that query</param>
/// <param name="Descending">true, if the field should be sorted in descending order</param>
public record SortDescriptor(
    string Property,
    bool Descending = false);