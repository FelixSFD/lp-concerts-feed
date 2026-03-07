using Database.Concerts;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure.Setlists;

namespace Lambda.SetlistsWrite.Services;

public class SetlistService(ISetlistRepository setlistRepository, IConcertRepository concertRepository)
{
    public async Task<CreateSetlistResponseDto> CreateSetlistAsync(CreateSetlistRequestDto request)
    {
        var setlistDo = new SetlistDo
        {
            ConcertId = request.ConcertId,
            LinkinpediaUrl = request.LinkinpediaUrl
        };
        
        setlistRepository.Add(setlistDo);
        await setlistRepository.SaveChangesAsync();

        var response = new CreateSetlistResponseDto
        {
            Id = setlistDo.Id,
            ConcertId = setlistDo.ConcertId,
            LinkinpediaUrl = setlistDo.LinkinpediaUrl
        };
        
        return response;
    }
}