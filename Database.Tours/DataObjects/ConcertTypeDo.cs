using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Database.DataObjects;

namespace Database.Tours.DataObjects;

[Table("ConcertType")]
public class ConcertTypeDo : BaseDo, ITimestampedDataObject, IEquatable<ConcertTypeDo>
{
    [Key]
    [Column("Id")]
    public uint Id { get; set; }

    [Column("Name")]
    [MaxLength(DataConstants.ConcertTypeNameLength)]
    public required string Name { get; set; }
    
    /// <inheritdoc/>
    [Column("CreatedAt")]
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <inheritdoc/>
    [Column("UpdatedAt")]
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <inheritdoc/>
    public bool Equals(ConcertTypeDo? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ConcertTypeDo)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return (int)Id;
    }
}