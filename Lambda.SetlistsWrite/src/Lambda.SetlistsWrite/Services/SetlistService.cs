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
    /// <param name="setlistId">ID of the setlist where the song wil be added to</param>
    /// <returns>the newly created setlist entry</returns>
    /// <exception cref="SetlistNotFoundException">if the setlist does not exist. Call <see cref="CreateSetlistAsync"/> to create a setlist first.</exception>
    public async Task<SetlistEntryDto> AddSongToSetlistAsync(AddSongToSetlistRequestDto request, uint setlistId)
    {
        logger.LogDebug("Load setlist: {setlistId}", setlistId);
        var setlist = await setlistRepository.GetByPrimaryKeyAsync(setlistId);
        if (setlist == null)
        {
            throw new SetlistNotFoundException(setlistId);
        }
        
        logger.LogDebug("Adding song to setlist: {setlistId}", setlist.Id);

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
            actDo = await actRepository.GetBy(setlist.Id, request.Act.ActNumber);
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

        var setlistEntryDto = SetlistEntryDoToDto(entry);
        return setlistEntryDto;
    }


    /// <summary>
    /// Returns a setlist with all entries
    /// </summary>
    /// <param name="setlistId">ID of the setlist</param>
    /// <returns></returns>
    /// <exception cref="SetlistNotFoundException">if the setlist was not found</exception>
    public async Task<SetlistDto?> GetCompleteSetlist(uint setlistId)
    {
        logger.LogDebug("Retrieve the setlist: {setlistId}", setlistId);
        var setlistDo = await setlistRepository.GetByPrimaryKeyAsync(setlistId) ?? throw new SetlistNotFoundException(setlistId);
        logger.LogDebug("Retrieved setlist: {setlistId}", setlistDo.Id);
        
        logger.LogDebug("{count} setlist entries", setlistDo.Entries?.Count);

        var setlistDto = new SetlistDto
        {
            Id = setlistDo.Id,
            ConcertId = setlistDo.ConcertId,
            LinkinpediaUrl = setlistDo.LinkinpediaUrl,
            Entries = setlistDo.Entries?.Select(SetlistEntryDoToDto).ToList() ?? []
        };
        
        return setlistDto;
    }


    private static SetlistEntryDto SetlistEntryDoToDto(SetlistEntryDo setlistEntry)
    {
        return new SetlistEntryDto
        {
            SongNumber = setlistEntry.SongNumber,
            SortNumber = setlistEntry.SortNumber,
            PlayedSong = setlistEntry.PlayedSong != null ? SongDoToDto(setlistEntry.PlayedSong) : null,
            Title = setlistEntry.TitleOverride ?? setlistEntry.PlayedSong?.Title ?? "unknown",
            ExtraNotes = setlistEntry.ExtraNotes,
            IsPlayedFromRecording = setlistEntry.IsPlayedFromRecording,
            IsRotationSong = setlistEntry.IsRotationSong,
            IsWorldPremiere = setlistEntry.IsWorldPremiere
        };
    }
    
    
    private static SongDto SongDoToDto(SongDo song)
    {
        return new SongDto
        {
            Id = song.Id,
            Title = song.Title,
            Isrc = song.Isrc
        };
    }
}