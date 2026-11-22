namespace Common.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
/// Interceptor that converts empty strings to null values before saving changes to the database.
/// This is useful for ensuring that string properties that are nullable in the database are not saved as empty strings, which can lead to inconsistencies and unexpected behavior.
/// </summary>
public sealed class EmptyStringToNullInterceptor : SaveChangesInterceptor
{
    private static void ConvertEmptyStringsToNull(DbContext? context)
    {
        if (context is null)
            return;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
                continue;

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType != typeof(string))
                    continue;

                if (!property.Metadata.IsNullable)
                    continue;

                if (property.CurrentValue is string { Length: 0 })
                    property.CurrentValue = null;
            }
        }
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ConvertEmptyStringsToNull(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ConvertEmptyStringsToNull(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}