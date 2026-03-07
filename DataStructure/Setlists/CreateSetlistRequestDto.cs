using System.ComponentModel.DataAnnotations;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Creates a new setlist for a concert
/// </summary>
public class CreateSetlistRequestDto
{
    /// <summary>
    /// ID of the <see cref="Concert"/> where this set was played
    /// </summary>
    public required string ConcertId { get; set; }
    
    /// <summary>
    /// URL to the wiki page on Linkinpedia
    /// </summary>
    [MaxLength(63)]
    public string? LinkinpediaUrl { get; set; }
}