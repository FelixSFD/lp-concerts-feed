namespace Lambda.CalendarFeed;

/// <summary>
/// Flags to specify which events to return
/// </summary>
[Flags]
public enum ConcertSubEventCategory
{
    // just put everything in one event (this is the behavior of the old feed)
    AsOneSingleEvent = 1 << 0,
    
    // include Linkin Park show
    LinkinPark = 1 << 2,
    
    // include Doors time
    Doors = 1 << 3,
    
    // include support acts if available
    SupportAct = 1 << 4
}