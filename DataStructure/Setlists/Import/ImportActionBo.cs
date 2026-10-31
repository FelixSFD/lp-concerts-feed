namespace LPCalendar.DataStructure.Setlists.Import;

/// <summary>
/// An action that has to be performed for the import
/// </summary>
public abstract class ImportActionBo(ImportActionType actionType)
{
    /// <summary>
    /// Type of the action
    /// </summary>
    public ImportActionType Type { protected get; set; } = actionType;
}