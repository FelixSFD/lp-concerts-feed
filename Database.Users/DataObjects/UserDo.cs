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
    [Column("Username")]
    public required string Username { get; set; }

    /// <summary>
    /// ISO code of the country where this user is from. Can be null if the user has not specified their country
    /// </summary>
    [MinLength(3)]
    [MaxLength(3)]
    [Column("OriginCountryCode")]
    public string? OriginCountryCode { get; set; }

    /// <inheritdoc/>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <inheritdoc/>
    public DateTimeOffset? UpdatedAt { get; set; }
}