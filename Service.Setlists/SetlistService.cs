using Amazon.Lambda.Core;
using Database.Concerts;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure.Setlists;
using Service.Setlists.Exceptions;

namespace Service.Setlists;

public class SetlistService(
    ISetlistRepository setlistRepository,
    ISetlistEntryRepository setlistEntryRepository,
    IConcertRepository concertRepository,
    ISongRepository songRepository,
    ISongVariantRepository songVariantRepository,
    ISongMashupRepository songMashupRepository,
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
            ConcertTitle = request.ConcertTitle,
            SetName = request.SetName,
            LinkinpediaUrl = request.LinkinpediaUrl
        };
        
        setlistRepository.Add(setlistDo);
        await setlistRepository.SaveChangesAsync();

        var response = new CreateSetlistResponseDto
        {
            Id = setlistDo.Id,
            ConcertId = setlistDo.ConcertId,
            ConcertTitle = setlistDo.ConcertTitle,
            SetName = setlistDo.SetName,
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
    /// <param name="playedSongMashup">Song mashup that was played. One of these parameters should be set</param>
    /// <returns>the newly created setlist entry</returns>
    /// <exception cref="SetlistNotFoundException">if the setlist does not exist. Call <see cref="CreateSetlistAsync"/> to create a setlist first.</exception>
    private async Task<SetlistEntryDto> AddToSetlist(AddToSetlistRequestDto request, uint setlistId, SongDo? playedSong = null, SongVariantDo? playedSongVariant = null, SongMashupDo? playedSongMashup = null)
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
            PlayedMashup = playedSongMashup,
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

        var setlistEntryDto = DtoMapper.ToDto(entry);
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
        if (songParams.SongId is > 0)
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
        if (songVariantParams.SongVariantId is > 0)
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
    /// Adds a new song mashup to the setlist
    /// </summary>
    /// <param name="request">Request to add a song mashup to the setlist</param>
    /// <param name="setlistId">ID of the setlist where the song mashup wil be added to</param>
    /// <returns>the newly created setlist entry</returns>
    /// <exception cref="SetlistNotFoundException">if the setlist does not exist. Call <see cref="CreateSetlistAsync"/> to create a setlist first.</exception>
    /// <exception cref="SongMashupNotFoundException">if the provided mashup does not exist. Call <see cref="SongService.CreateMashupOfSongsAsync"/> to create a <see cref="SongMashupDo"/> first.</exception>
    public async Task<SetlistEntryDto> AddSongMashupToSetlistAsync(AddSongMashupToSetlistRequestDto request, uint setlistId)
    {
        logger.LogDebug("Load setlist: {setlistId}", setlistId);
        var setlist = await setlistRepository.GetByPrimaryKeyAsync(setlistId);
        if (setlist == null)
        {
            throw new SetlistNotFoundException(setlistId);
        }
        
        logger.LogDebug("Adding song to setlist: {setlistId}", setlist.Id);

        var songMashupParams = request.SongMashupParameters;
        logger.LogDebug("Checking if song mashup exists: {songMashupId}", songMashupParams.SongMashupId);
        var songMashup = await songMashupRepository.GetByPrimaryKeyAsync(songMashupParams.SongMashupId);
        if (songMashup == null)
        {
            throw new SongMashupNotFoundException(songMashupParams.SongMashupId);
        }

        logger.LogDebug("Found the mashup. Will add it to the setlist now...");
        return await AddToSetlist(request, setlistId, null, null, songMashup);
    }
    
    
    /// <summary>
    /// Returns a setlist with all entries
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    /// <exception cref="SetlistNotFoundException">if the setlist was not found</exception>
    public IAsyncEnumerable<SetlistHeaderDto> GetSetlistHeaders(CancellationToken cancellationToken)
    {
        logger.LogDebug("Retrieve setlist headers");
        return setlistRepository
            .QueryAsync(cancellationToken)
            .Select(DtoMapper.ToHeaderOnlyDto);
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
        return DtoMapper.ToDto(setlistDo);
    }
    
    /// <summary>
    /// Returns all setlists for a given concert. A concert usually only has one setlists, but can technically have more than one. (for example rehearsals)
    /// </summary>
    /// <param name="concertId">ID of the concert</param>
    /// <returns></returns>
    /// <exception cref="SetlistNotFoundException">if the setlist was not found</exception>
    public async Task<IList<SetlistDto>> GetSetlistsForConcert(string concertId)
    {
        logger.LogDebug("Retrieve the setlists for concert: {concertId}", concertId);
        return await setlistRepository
            .GetByConcertIdAsync(concertId)
            .Select(sl =>
            {
                foreach (var setlistEntryDo in sl.Entries)
                {
                    logger.LogDebug("Act: {actNumber}", setlistEntryDo.Act?.ActNumber);
                    logger.LogDebug("Entry with Song ID: {id} (Title: {title})", setlistEntryDo.PlayedSong?.Id, setlistEntryDo.PlayedSong?.Title);
                    logger.LogDebug("Entry with Song variant ID: {id} (Title: {title}; Variant: {variantName})", setlistEntryDo.PlayedSongVariant?.Id, setlistEntryDo.PlayedSongVariant?.Song.Title, setlistEntryDo.PlayedSongVariant?.VariantName);
                    logger.LogDebug("Entry with Song mashup ID: {id}(Title: {title})", setlistEntryDo.PlayedMashup?.Id, setlistEntryDo.PlayedMashup?.Title);
                }

                return sl;
            })
            .Select(DtoMapper.ToDto)
            .ToListAsync();
    }


    /// <summary>
    /// Updates the header information of a setlist
    /// </summary>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="request">Updated data</param>
    /// <exception cref="SetlistNotFoundException">if the setlist does not exist</exception>
    public async Task UpdateSetlistHeader(uint setlistId, UpdateSetlistHeaderRequestDto request)
    {
        logger.LogDebug("Load setlist: {setlistId}", setlistId);
        var setlist = await setlistRepository.GetByPrimaryKeyAsync(setlistId) ?? throw new SetlistNotFoundException(setlistId);

        setlist.SetName = request.SetName;
        setlist.LinkinpediaUrl = request.LinkinpediaUrl;
        
        setlistRepository.Update(setlist);
        await setlistRepository.SaveChangesAsync();
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
}