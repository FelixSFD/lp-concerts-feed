using LPCalendar.DataStructure;

namespace Service.Setlists;

public interface ISetlistPushEventSender
{
    Task SendLivePremiere(string setlistEntryTitle, ConcertDto concert);
}