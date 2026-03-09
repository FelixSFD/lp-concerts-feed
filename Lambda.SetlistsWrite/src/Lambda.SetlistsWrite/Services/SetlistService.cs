using Amazon.Lambda.Core;
using Database.Concerts;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using Lambda.SetlistsWrite.Services.Exceptions;
using LPCalendar.DataStructure.Setlists;

namespace Lambda.SetlistsWrite.Services;

public class SetlistService(
    ISetlistRepository setlistRepository,
    ISetlistEntryRepository setlistEntryRepository,
    IConcertRepository concertRepository,
    ISongRepository songRepository,
    ISongVariantRepository songVariantRepository,
    ISetlistActRepository actRepository,
    ILambdaLogger logger)
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
    /// Adds a new entry to the setlist. This method should only be called if all the other data from the <paramref name="request"/> is already processed!
    /// </summary>
    /// <param name="request">Request to add a new entry to the setlist</param>
    /// <param name="setlistId">ID of the setlist where the song wil be added to</param>
    /// <param name="playedSong">Song that was played. One of these parameters should be set</param>
    /// <param name="playedSongVariant">Song variant that was played. One of these parameters should be set</param>
    /// <returns>the newly created setlist entry</returns>
    /// <exception cref="SetlistNotFoundException">if the setlist does not exist. Call <see cref="CreateSetlistAsync"/> to create a setlist first.</exception>
    private async Task<SetlistEntryDto> AddToSetlist(AddToSetlistRequestDto request, uint setlistId, SongDo? playedSong = null, SongVariantDo? playedSongVariant = null)
    {
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
            actDo = await actRepository.GetBy(setlistId, request.Act.ActNumber);
            if (actDo == null)
            {
                logger.LogDebug("Adding act {actNumber} to setlist", request.Act);
                actDo = new SetlistActDo
                {
                    SetlistId = setlistId,
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
            SetlistId = setlistId,
            SongNumber = request.EntryParameters.SongNumber,
            Act = actDo,
            ActNumber = actDo?.ActNumber,
            PlayedSong = playedSong,
            PlayedSongVariant = playedSongVariant,
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

        return await AddToSetlist(request, setlistId, song);
    }
    
    
    /// <summary>
    /// Adds a new song variant to the setlist and creates the <see cref="SongVariantDo"/> if it does not exist yet.
    /// </summary>
    /// <param name="request">Request to add a new song variant to the setlist</param>
    /// <param name="setlistId">ID of the setlist where the song variant wil be added to</param>
    /// <returns>the newly created setlist entry</returns>
    /// <exception cref="SetlistNotFoundException">if the setlist does not exist. Call <see cref="CreateSetlistAsync"/> to create a setlist first.</exception>
    public async Task<SetlistEntryDto> AddSongVariantToSetlistAsync(AddSongVariantToSetlistRequestDto request, uint setlistId)
    {
        logger.LogDebug("Load setlist: {setlistId}", setlistId);
        var setlist = await setlistRepository.GetByPrimaryKeyAsync(setlistId);
        if (setlist == null)
        {
            throw new SetlistNotFoundException(setlistId);
        }
        
        logger.LogDebug("Adding song to setlist: {setlistId}", setlist.Id);

        var songVariantParams = request.SongVariantParameters;
        SongVariantDo? songVariant;
        if (songVariantParams.SongVariantId != null)
        {
            logger.LogDebug("Checking if song variant exists: {songVariantId}", songVariantParams.SongVariantId);
            songVariant = await songVariantRepository.GetByPrimaryKeyAsync(songVariantParams.SongVariantId ?? 0);
        }
        else
        {
            logger.LogDebug("Create a new song variant");
            songVariant = new SongVariantDo
            {
                SongId = songVariantParams.SongId ?? 0,
                VariantName = songVariantParams.VariantName,
                Description = songVariantParams.Description,
                IsrcOverride = songVariantParams.IsrcOverride
            };
            
            songVariantRepository.Add(songVariant);
        }

        return await AddToSetlist(request, setlistId, null, songVariant);
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
    
    /// <summary>
    /// Removes a setlist
    /// </summary>
    /// <param name="setlistId">ID of the setlist to delete</param>
    public async Task RemoveSetlist(uint setlistId)
    {
        var entryDo = await setlistRepository.GetByPrimaryKeyAsync(setlistId);
        if (entryDo != null)
            await RemoveSetlist(entryDo);
    }
    
    /// <summary>
    /// Removes a setlist
    /// </summary>
    /// <param name="setlist">setlist to delete</param>
    private async Task RemoveSetlist(SetlistDo setlist)
    {
        setlistRepository.Delete(setlist);
        await setlistRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Removes an entry from the setlist
    /// </summary>
    /// <param name="setlistEntryId">ID of the entry to delete</param>
    public async Task RemoveSetlistEntry(string setlistEntryId)
    {
        var entryDo = await setlistEntryRepository.GetByPrimaryKeyAsync(setlistEntryId);
        if (entryDo != null)
            await RemoveSetlistEntry(entryDo);
    }
    
    /// <summary>
    /// Removes an entry from the setlist
    /// </summary>
    /// <param name="setlistEntry">Entry to delete</param>
    private async Task RemoveSetlistEntry(SetlistEntryDo setlistEntry)
    {
        setlistEntryRepository.Delete(setlistEntry);
        await setlistEntryRepository.SaveChangesAsync();
    }


    private static SetlistEntryDto SetlistEntryDoToDto(SetlistEntryDo setlistEntry)
    {
        return new SetlistEntryDto
        {
            Id = setlistEntry.Id,
            SongNumber = setlistEntry.SongNumber,
            SortNumber = setlistEntry.SortNumber,
            PlayedSong = setlistEntry.PlayedSong != null ? SongDoToDto(setlistEntry.PlayedSong) : null,
            PlayedSongVariant = DtoMapper.ToDtoNullable(setlistEntry.PlayedSongVariant),
            Title = setlistEntry.TitleOverride ?? GetEntryTitleForSongVariant(setlistEntry.PlayedSong, setlistEntry.PlayedSongVariant) ?? setlistEntry.PlayedSong?.Title ?? "unknown",
            ExtraNotes = setlistEntry.ExtraNotes,
            IsPlayedFromRecording = setlistEntry.IsPlayedFromRecording,
            IsRotationSong = setlistEntry.IsRotationSong,
            IsWorldPremiere = setlistEntry.IsWorldPremiere
        };
    }


    private static string? GetEntryTitleForSongVariant(SongDo? songDo, SongVariantDo? songVariantDo)
    {
        if (songDo == null || songVariantDo == null)
            return null;

        return $"{songDo} ({songVariantDo})";
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