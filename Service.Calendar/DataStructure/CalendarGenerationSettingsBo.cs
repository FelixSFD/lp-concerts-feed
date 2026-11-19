namespace Service.Calendar.DataStructure;

/// <summary>Controls which detailed events are included in a concert calendar.</summary>
/// <remarks>Map the corresponding calendar subscription settings into this object before generation.</remarks>
public sealed class CalendarGenerationSettingsBo
{
    /// <summary>Whether to include a doors opening event when its time is known.</summary>
    public bool IncludeDoorsTime { get; set; }

    /// <summary>Whether to include a Linkin Park stage time event when its time is known.</summary>
    public bool IncludeLpStageTime { get; set; }
}
