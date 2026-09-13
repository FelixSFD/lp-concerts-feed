namespace Common.WikiMedia;

public class CargoQueryWhereClause
{
    public required string FieldName { get; set; }
    public required string Value { get; set; }

    public Operation Comparison { get; set; }

    public override string ToString()
    {
        return $"{FieldName} {GetComparisonString(Comparison)} \"{Value}\"";
    }

    private static string GetComparisonString(Operation operation)
    {
        return operation switch
        {
            Operation.IsEqual => "=",
            Operation.IsNotEqual => "!=",
            Operation.IsGreaterThan => ">",
            Operation.IsLessThan => "<",
            Operation.IsGreaterThanOrEqual => ">=",
            Operation.IsLessThanOrEqual => "<=",
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
    }


    public enum Operation
    {
        IsEqual,
        IsNotEqual,
        IsGreaterThan,
        IsLessThan,
        IsGreaterThanOrEqual,
        IsLessThanOrEqual,
    }
}