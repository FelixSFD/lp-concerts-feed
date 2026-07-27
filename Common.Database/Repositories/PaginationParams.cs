namespace Common.Database.Repositories;

/// <inheritdoc/>
public class PaginationParams(uint skip, uint take) : IPaginationParams
{
    /// <inheritdoc/>
    public uint Skip { get; set; } = skip;

    /// <inheritdoc/>
    public uint Take { get; set; } = take;
}