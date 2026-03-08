using Amazon.Lambda.Core;
using Database.Concerts;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using Lambda.SetlistsWrite.Services.Exceptions;
using LPCalendar.DataStructure.Setlists;

namespace Lambda.SetlistsWrite.Services;

public class SetlistService(ISetlistRepository setlistRepository, ISetlistEntryRepository setlistEntryRepository, IConcertRepository concertRepository, ISongRepository songRepository, ISetlistActRepository actRepository, ILambdaLogger logger)
{
    /// <summary>
    /// Creates a new (empty) setlist in the database
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
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


    /// <summary>
    /// Adds a new song to the setlist and creates the <see cref="SongDo"/> if it does not exist yet.
    /// </summary>
    /// <param name="request">Request to add a new song to the setlist</param>
    /// <returns>the newly created setlist entry</returns>
    /// <exception cref="SetlistNotFoundException">if the setlist does not exist. Call <see cref="CreateSetlistAsync"/> to create a setlist first.</exception>
    public async Task<SetlistEntryDto?> AddSongToSetlistAsync(AddSongToSetlistRequestDto request)
    {
        logger.LogDebug("Load setlist: {setlistId}", request.SetlistId);
        var setlist = await setlistRepository.GetByPrimaryKeyAsync(request.SetlistId);
        if (setlist == null)
        {
            throw new SetlistNotFoundException(request.SetlistId); 
        }
        
        logger.LogDebug("Adding song to setlist: {setlistId}", request.SetlistId);

        var songParams = request.SongParameters;
        SongDo? song;
        if (songParams.SongId > 0)
        {
            logger.LogDebug("Checking if song exists: {songId}", songParams.SongId);
            song = await songRepository.GetByPrimaryKeyAsync(songParams.SongId ?? 0);
        }
        else
        {
            logger.LogDebug("Create a new song");
            song = new SongDo
            {
                Title = songParams.SongTitle!,
                Isrc = songParams.Isrc
            };
            
            songRepository.Add(song);
        }
        
        logger.LogDebug("Read or create setlist act: {actNumber}");
        SetlistActDo? actDo;
        if (request.Act == null)
        {
            logger.LogDebug("This entry does not belong to any act.");
            actDo = null;
        }
        else
        {
            logger.LogDebug("Checking if act exists: {actNumber}", request.Act);
            actDo = await actRepository.GetBy(request.SetlistId, request.Act.ActNumber);
            if (actDo == null)
            {
                logger.LogDebug("Adding act {actNumber} to setlist", request.Act);
                actDo = new SetlistActDo
                {
                    SetlistId = setlist.Id,
                    ActNumber = request.Act.ActNumber,
                    Title = request.Act.Title
                };
                
                actRepository.Add(actDo);
                logger.LogDebug("Added act.");
            }
            else
            {
                logger.LogDebug("Read act: {actNumber}; Title: {title}", actDo.ActNumber, actDo.Title);
            }
        }
        
        logger.LogDebug("Creating setlist entry...");
        var entry = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            SetlistId = setlist.Id,
            PlayedSong = song,
            Act = actDo,
            SongNumber = request.EntryParameters.SongNumber,
            ExtraNotes = request.EntryParameters.ExtraNotes,
            TitleOverride = request.EntryParameters.TitleOverride,
            SortNumber = request.EntryParameters.SortNumber,
            IsWorldPremiere = request.EntryParameters.IsWorldPremiere,
            IsPlayedFromRecording = request.EntryParameters.IsPlayedFromRecording,
            IsRotationSong = request.EntryParameters.IsRotationSong
        };
        
        setlistEntryRepository.Add(entry);
        
        logger.LogDebug("Saving...");
        await setlistEntryRepository.SaveChangesAsync();
        logger.LogDebug("Successfully saved.");

        var setlistEntryDto = new SetlistEntryDto
        {
            SongNumber = entry.SongNumber,
            SortNumber = entry.SortNumber,
            TitleOverride = entry.TitleOverride,
            ExtraNotes = entry.ExtraNotes,
            IsPlayedFromRecording = entry.IsPlayedFromRecording,
            IsRotationSong = entry.IsRotationSong,
            IsWorldPremiere = entry.IsWorldPremiere
        };

        return setlistEntryDto;
    }
}