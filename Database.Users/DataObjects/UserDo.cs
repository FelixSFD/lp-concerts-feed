using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Database.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Users.DataObjects;

/// <summary>
/// Information about a user
/// </summary>
[Table("User")]
[PrimaryKey(nameof(Id))]
[Index(nameof(Username), Name = "Unique_Username", IsUnique = true)]
public class UserDo : BaseDo, ITimestampedDataObject
{
    /// <summary>
    /// Unique ID of this user
    /// </summary>
    [Key]
    [Column("Id")]
    [MaxLength(DataConstants.UserIdLength)]
    public required string Id { get; set; }

    /// <summary>
    /// Displayed name of this user
    /// </summary>
    [MaxLength(DataConstants.UsernameLength)]
    public required string Username { get; set; }

    /// <inheritdoc/>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <inheritdoc/>
    public DateTimeOffset? UpdatedAt { get; set; }
}