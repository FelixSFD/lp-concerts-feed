using Database.Tours.DataObjects;

namespace Database.Tours.Extensions;


public static class PreviousNamesCollectionExtension
{
    /// <summary>
    /// Returns the name from the collection that is valid at <paramref name="validAt"/>
    /// </summary>
    /// <param name="previousNames">Enumerable of names</param>
    /// <param name="validAt">Time at which the name should be valid</param>
    /// <returns>the name from the collection that is valid at <paramref name="validAt"/></returns>
    public static PreviousVenueNameDo GetValidNameEntryAt(this IEnumerable<PreviousVenueNameDo> previousNames, DateTimeOffset validAt)
    {
        var checkDate = DateOnly.FromDateTime(validAt.UtcDateTime);
        return previousNames
            .Where(pn => pn.From <= checkDate && (pn.To ?? DateOnly.MaxValue) >= checkDate)
            .OrderBy(pn  => pn.From)
            .Last();
    }
}