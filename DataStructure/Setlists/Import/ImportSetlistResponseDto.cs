namespace LPCalendar.DataStructure.Setlists.Import;

/// <summary>
/// Response for when the import was executed
/// </summary>
public class ImportSetlistResponseDto
{
    public required ImportActionDto[] Actions { get; set; }
}