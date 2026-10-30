namespace LPCalendar.DataStructure.Setlists.Import;

/// <summary>
/// Response for when the import was executed
/// </summary>
public class ImportSetlistResponseBo
{
    public required ImportActionBo[] Actions { get; set; }
}