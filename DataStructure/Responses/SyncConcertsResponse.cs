namespace LPCalendar.DataStructure.Responses;


/// <summary>
/// Response with new data to sync.
/// </summary>
public class SyncConcertsResponse
{
    /// <summary>
    /// Time of the sync. Use this time for the next sync-requests.
    /// </summary>
    public required DateTimeOffset SyncTime { get; set; }
    
    /// <summary>
    /// These concerts have been added
    /// </summary>
    public required Concert[] Added { get; set; }
    
    /// <summary>
    /// These concerts have been updated
    /// </summary>
    public required Concert[] Updated { get; set; }
    
    /// <summary>
    /// Concerts with this ID were present in the request but not found in the database.
    /// They need to be deleted
    /// </summary>
    public required string[] DeletedConcertIds { get; set; }
}