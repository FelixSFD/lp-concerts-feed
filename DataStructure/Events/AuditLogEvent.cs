namespace LPCalendar.DataStructure.Events;

public class AuditLogEvent
{
    /// <summary>
    /// ID of the user who performed the action
    /// </summary>
    public string UserId { get; set; }
    
    /// <summary>
    /// Action the user performed
    /// </summary>
    public string Action { get; set; }
    
    /// <summary>
    /// If applicable, a description if the entity that was modified
    /// </summary>
    public string? AffectedEntity { get; set; }
    
    /// <summary>
    /// Timestamp of the action
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}