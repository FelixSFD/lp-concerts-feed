using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.DataObjects;

/// <summary>
/// Objects that have creation and update timestamps in the database
/// </summary>
public interface ITimestampedDataObject
{
    /// <summary>
    /// Time when this entry was created
    /// </summary>
    [Column("CreatedAt")]
    DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Time when this entry was last updated
    /// </summary>
    [Column("UpdatedAt")]
    DateTimeOffset? UpdatedAt { get; set; }
}
