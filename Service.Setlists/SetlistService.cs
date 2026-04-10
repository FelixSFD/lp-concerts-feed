using System.Text.Json;
using Amazon.Lambda.Core;
using Common.Utils;
using Database.Concerts;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure.Setlists;
using LPCalendar.DataStructure.Setlists.Parameters;
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
        logger.LogDebug("Read Concert information: {concertId}", request.ConcertId);
        var concert = await concertRepository.GetByIdAsync(request.ConcertId) ?? throw new ConcertNotFoundException(request.ConcertId);
        
        var setlistDo = new SetlistDo
        {
            ConcertId = request.ConcertId,
            ConcertTitle = request.ConcertTitle,
            ConcertType = concert.ShowType,
            ConcertTourName = concert.TourName,
            ConcertDate = concert.PostedStartTime!.Value.Date,
            SetName = StringUtils.NullIfEmpty(request.SetName),
            LinkinpediaUrl = StringUtils.NullIfEmpty(request.LinkinpediaUrl)
        };
        
        setlistRepository.Add(setlistDo);
        await setlistRepository.SaveChangesAsync();
        
        if (request.SongEntries != null)
        {
            logger.LogDebug("Song entries: {count}", request.SongEntries.Count);
            await foreach (var addEntryParams in request.SongEntries.ToAsyncEnumerable())
            {
                await AddSongToSetlistAsync(addEntryParams, setlistDo.Id, false);
            }
        }
        
        if (request.SongVariantEntries != null)
        {
            logger.LogDebug("Song variant entries: {count}", request.SongVariantEntries.Count);
            await foreach (var addEntryParams in request.SongVariantEntries.ToAsyncEnumerable())
            {
                await AddSongVariantToSetlistAsync(addEntryParams, setlistDo.Id, false);
            }
        }
        
        if (request.MashupEntries != null)
        {
            logger.LogDebug("Song mashup entries: {count}", request.MashupEntries.Count);
            await foreach (var addEntryParams in request.MashupEntries.ToAsyncEnumerable())
            {
                await AddSongMashupToSetlistAsync(addEntryParams, setlistDo.Id, false);
            }
        }

        if (request.SongEntries?.Count > 0 || request.SongVariantEntries?.Count > 0 || request.MashupEntries?.Count > 0)
        {
            logger.LogDebug("Saving entries...");
            await setlistEntryRepository.SaveChangesAsync();
            logger.LogDebug("Saved entries!");
        }

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
    /// <param name="saveContext">true, if the DbContext should be saved automatically</param>
    /// <returns>the newly created setlist entry</returns>
    /// <exception cref="SetlistNotFoundException">if the setlist does not exist. Call <see cref="CreateSetlistAsync"/> to create a setlist first.</exception>
    private async Task<SetlistEntryDto> AddToSetlist(AddToSetlistRequestDto request, uint setlistId, SongDo? playedSong = null, SongVariantDo? playedSongVariant = null, SongMashupDo? playedSongMashup = null, bool saveContext = true)
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
            logger.LogDebug("Checking if act exists: {actNumber}", request.Act!.ActNumber);
            actDo = await GetOrAddFromParameters(request.Act!, setlistId);
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
            ExtraNotes = StringUtils.NullIfEmpty(request.EntryParameters.ExtraNotes),
            TitleOverride = StringUtils.NullIfEmpty(request.EntryParameters.TitleOverride),
            SortNumber = request.EntryParameters.SortNumber,
            IsWorldPremiere = request.EntryParameters.IsWorldPremiere,
            IsPlayedFromRecording = request.EntryParameters.IsPlayedFromRecording,
            IsRotationSong = request.EntryParameters.IsRotationSong,
            IsLivePremiere = request.EntryParameters.IsLivePremiere
        };
        
        setlistEntryRepository.Add(entry);

        if (saveContext)
        {
            logger.LogDebug("Saving...");
            await setlistEntryRepository.SaveChangesAsync();
            logger.LogDebug("Successfully saved.");
        
            await UpdateSetlistCacheForSetlist(setlistId, DateTimeOffset.Now);
        }

        var setlistEntryDto = DtoMapper.ToDto(entry);
        return setlistEntryDto;
    }


    /// <summary>
    /// Adds a new song to the setlist and creates the <see cref="SongDo"/> if it does not exist yet.
    /// </summary>
    /// <param name="request">Request to add a new song to the setlist</param>
    /// <param name="setlistId">ID of the setlist where the song wil be added to</param>
    /// <param name="saveContext">true, if the DbContext should be saved automatically</param>
    /// <returns>the newly created setlist entry</returns>
    /// <exception cref="SetlistNotFoundException">if the setlist does not exist. Call <see cref="CreateSetlistAsync"/> to create a setlist first.</exception>
    public async Task<SetlistEntryDto> AddSongToSetlistAsync(AddSongToSetlistRequestDto request, uint setlistId, bool saveContext = true)
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
                Isrc = StringUtils.NullIfEmpty(songParams.Isrc)
            };
            
            songRepository.Add(song);
        }

        return await AddToSetlist(request, setlistId, song, saveContext: saveContext);
    }
    
    
    /// <summary>
    /// Adds a new song variant to the setlist and creates the <see cref="SongVariantDo"/> if it does not exist yet.
    /// </summary>
    /// <param name="request">Request to add a new song variant to the setlist</param>
    /// <param name="setlistId">ID of the setlist where the song variant wil be added to</param>
    /// <param name="saveContext">true, if the DbContext should be saved automatically</param>
    /// <returns>the newly created setlist entry</returns>
    /// <exception cref="SetlistNotFoundException">if the setlist does not exist. Call <see cref="CreateSetlistAsync"/> to create a setlist first.</exception>
    public async Task<SetlistEntryDto> AddSongVariantToSetlistAsync(AddSongVariantToSetlistRequestDto request, uint setlistId, bool saveContext = true)
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
                VariantName = StringUtils.NullIfEmpty(songVariantParams.VariantName),
                Description = StringUtils.NullIfEmpty(songVariantParams.Description),
                IsrcOverride = StringUtils.NullIfEmpty(songVariantParams.IsrcOverride)
            };
            
            songVariantRepository.Add(songVariant);
        }

        return await AddToSetlist(request, setlistId, null, songVariant, saveContext: saveContext);
    }
    
    
    /// <summary>
    /// Adds a new song mashup to the setlist
    /// </summary>
    /// <param name="request">Request to add a song mashup to the setlist</param>
    /// <param name="setlistId">ID of the setlist where the song mashup wil be added to</param>
    /// <param name="saveContext">true, if the DbContext should be saved automatically</param>
    /// <returns>the newly created setlist entry</returns>
    /// <exception cref="SetlistNotFoundException">if the setlist does not exist. Call <see cref="CreateSetlistAsync"/> to create a setlist first.</exception>
    /// <exception cref="SongMashupNotFoundException">if the provided mashup does not exist. Call <see cref="SongService.CreateMashupOfSongsAsync"/> to create a <see cref="SongMashupDo"/> first.</exception>
    public async Task<SetlistEntryDto> AddSongMashupToSetlistAsync(AddSongMashupToSetlistRequestDto request, uint setlistId, bool saveContext = true)
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
        return await AddToSetlist(request, setlistId, null, null, songMashup, saveContext);
    }


    /// <summary>
    /// Returns all acts within a setlist
    /// </summary>
    /// <param name="setlistId">ID of the setlist</param>
    /// <returns></returns>
    /// <exception cref="SetlistNotFoundException">if the setlist does not exist</exception>
    public async IAsyncEnumerable<SetlistActDto> GetActsWithinSetlistAsync(uint setlistId)
    {
        var setlist = await setlistRepository.GetByPrimaryKeyAsync(setlistId) ?? throw new SetlistNotFoundException(setlistId);
        logger.LogDebug("Found setlist with {entries} entries.", setlist.Entries.Count);
        
        var acts = setlist
            .Entries
            .GroupBy(e => e.Act)
            .Select(g => g.Key)
            .Where(act => act != null)
            .Cast<SetlistActDo>()
            .Select(DtoMapper.ToDto);

        logger.LogDebug("Start returning the acts...");
        foreach (var act in acts)
        {
            yield return act;
        }
        logger.LogDebug("Finished returning the acts.");
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
    /// Returns a setlist entry
    /// </summary>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="entryId">ID of the entry</param>
    /// <returns></returns>
    /// <exception cref="SetlistEntryNotFoundException">if the setlist entry was not found</exception>
    public async Task<RawSetlistEntryDto> GetSetlistEntryAsync(uint setlistId, string entryId)
    {
        logger.LogDebug("Retrieve the setlist entry: {entryId}", entryId);
        var entry = await setlistEntryRepository.GetByPrimaryKeyAsync(entryId) ?? throw new SetlistEntryNotFoundException(setlistId, entryId);
        return DtoMapper.ToRawDto(entry);
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
            .Select(DebugSetlistEntries)
            .Select(DtoMapper.ToDto)
            .ToListAsync();
    }

    private SetlistDo DebugSetlistEntries(SetlistDo sl)
    {
        logger.LogDebug("DEBUG the setlist entries for setlist: {setlistId}", sl.Id);
        foreach (var setlistEntryDo in sl.Entries)
        {
            logger.LogDebug("Act: {actNumber}", setlistEntryDo.Act?.ActNumber);
            logger.LogDebug("Entry with Song ID: {id} (Title: {title})", setlistEntryDo.PlayedSong?.Id, setlistEntryDo.PlayedSong?.Title);
            logger.LogDebug("Entry with Song variant ID: {id} (Title: {title}; Variant: {variantName})", setlistEntryDo.PlayedSongVariant?.Id, setlistEntryDo.PlayedSongVariant?.Song.Title, setlistEntryDo.PlayedSongVariant?.VariantName);
            logger.LogDebug("Entry with Song mashup ID: {id}(Title: {title})", setlistEntryDo.PlayedMashup?.Id, setlistEntryDo.PlayedMashup?.Title);
        }

        return sl;
    }


    /// <summary>
    /// Updates the header information of a setlist
    /// </summary>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="request">Updated data</param>
    /// <exception cref="SetlistNotFoundException">if the setlist does not exist</exception>
    public async Task UpdateSetlistHeader(uint setlistId, UpdateSetlistHeaderRequestDto request)
    {
        logger.LogDebug("Load setlist '{setlistId}' and concert '{concertId}'", setlistId, request.ConcertId);
        var (setlist, concert) = (await setlistRepository.GetByPrimaryKeyAsync(setlistId) ?? throw new SetlistNotFoundException(setlistId), await concertRepository.GetByIdAsync(request.ConcertId) ?? throw new ConcertNotFoundException(request.ConcertId));
        logger.LogDebug("Finished loading setlist and concert.");
        
        // update setlist fields
        setlist.SetName = request.SetName;
        setlist.LinkinpediaUrl = request.LinkinpediaUrl;
        
        // set concert related fields
        setlist.ConcertType = concert.ShowType;
        setlist.ConcertDate = concert.PostedStartTime!.Value.Date;
        setlist.ConcertTourName = concert.TourName;
        
        setlistRepository.Update(setlist);
        await setlistRepository.SaveChangesAsync();
        
        await UpdateSetlistCacheForConcert(setlist.ConcertId, DateTimeOffset.Now);
    }
    
    /// <summary>
    /// Updates the information of a setlist entry
    /// </summary>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="entryId">ID of the setlist entry</param>
    /// <param name="request">Updated data</param>
    /// <exception cref="SetlistEntryNotFoundException">if the setlist entry does not exist</exception>
    public async Task UpdateSetlistEntryAsync(uint setlistId, string entryId, UpdateSetlistEntryRequestDto request)
    {
        logger.LogDebug("Load setlist entry: {entryId}", entryId);
        var setlistEntry = await setlistEntryRepository.GetByPrimaryKeyAsync(entryId) ?? throw new SetlistEntryNotFoundException(setlistId, entryId);
        
        // update general parameters
        var entryParams = request.EntryParameters;
        setlistEntry.SongNumber = entryParams.SongNumber;
        setlistEntry.SortNumber = entryParams.SortNumber;
        setlistEntry.TitleOverride = entryParams.TitleOverride;
        setlistEntry.ExtraNotes = entryParams.ExtraNotes;
        setlistEntry.IsPlayedFromRecording = entryParams.IsPlayedFromRecording;
        setlistEntry.IsRotationSong = entryParams.IsRotationSong;
        setlistEntry.IsWorldPremiere = entryParams.IsWorldPremiere;
        setlistEntry.IsLivePremiere = entryParams.IsLivePremiere;
        
        // update act
        var actParams = request.ActParameters;
        if (actParams != null)
        {
            setlistEntry.Act = await GetOrAddFromParameters(actParams, setlistId);
            setlistEntry.ActNumber = actParams.ActNumber;
        }
        else
        {
            setlistEntry.Act = null;
            setlistEntry.ActNumber = null;
        }
        
        // update song
        var songParams = request.SongParameters;
        if (songParams != null)
        {
            logger.LogDebug("This entry contains a song.");
            setlistEntry.PlayedSong = await GetOrAddFromParameters(songParams);
            setlistEntry.PlayedSongId = setlistEntry.PlayedSong?.Id;
        }
        else
        {
            setlistEntry.PlayedSong = null;
            setlistEntry.PlayedSongId = null;
        }
        
        // update variant
        var songVariantParams = request.SongVariantParameters;
        if (songVariantParams != null)
        {
            logger.LogDebug("This entry contains a song variant.");
            setlistEntry.PlayedSongVariant = await GetOrAddFromParameters(songVariantParams);
            setlistEntry.PlayedSongVariantId = setlistEntry.PlayedSongVariant?.Id;
        }
        else
        {
            setlistEntry.PlayedSongVariant = null;
            setlistEntry.PlayedSongVariantId = null;
        }
        
        // update mashup
        var songMashupParams = request.SongMashupParameters;
        if (songMashupParams != null)
        {
            logger.LogDebug("This entry contains a song mashup.");
            var foundMashup = await songMashupRepository.GetByPrimaryKeyAsync(songMashupParams.SongMashupId) ?? throw new SongMashupNotFoundException(songMashupParams.SongMashupId);
            setlistEntry.PlayedMashup = foundMashup;
            setlistEntry.PlayedMashupId = foundMashup.Id;
        }
        else
        {
            setlistEntry.PlayedMashup = null;
            setlistEntry.PlayedMashupId = null;
        }
        
        setlistEntryRepository.Update(setlistEntry);
        await setlistEntryRepository.SaveChangesAsync();
        logger.LogDebug("Updated setlist entry: {entryId}", entryId);
        
        await UpdateSetlistCacheForSetlist(setlistId, DateTimeOffset.Now);
    }
    
    /// <summary>
    /// Either creates a new <see cref="SetlistActDo"/> or returns the stored object for the <see cref="ActParametersDto.ActNumber"/>
    /// </summary>
    /// <param name="actParams"></param>
    /// <param name="setlistId"></param>
    /// <returns></returns>
    /// <exception cref="SongNotFoundException">if an act number was passed, but the song does not exist</exception>
    private async Task<SetlistActDo> GetOrAddFromParameters(ActParametersDto actParams, uint setlistId)
    {
        var actDo = await actRepository.GetBy(setlistId, actParams.ActNumber);
        if (actDo == null)
        {
            logger.LogDebug("Adding act {actNumber} to setlist", actParams.ActNumber);
            actDo = new SetlistActDo
            {
                SetlistId = setlistId,
                ActNumber = actParams.ActNumber,
                Title = StringUtils.NullIfEmpty(actParams.Title)
            };
                
            actRepository.Add(actDo);
            logger.LogDebug("Added act.");
        }
        else
        {
            logger.LogDebug("Read act: {actNumber}; Title: {title}", actDo.ActNumber, actDo.Title);
        }
        
        return actDo;
    }

    /// <summary>
    /// Either creates a new <see cref="SongDo"/> or returns the stored object for the <see cref="SongParametersDto.SongId"/>
    /// </summary>
    /// <param name="songParams"></param>
    /// <returns></returns>
    /// <exception cref="SongNotFoundException">if a Song ID was passed, but the song does not exist</exception>
    private async Task<SongDo> GetOrAddFromParameters(SongParametersDto songParams)
    {
        SongDo song;
        if (songParams.SongId is > 0)
        {
            logger.LogDebug("Checking if song exists: {songId}", songParams.SongId);
            song = await songRepository.GetByPrimaryKeyAsync(songParams.SongId ?? 0) ?? throw new SongNotFoundException(songParams.SongId ?? 0);
        }
        else
        {
            logger.LogDebug("Create a new song");
            song = new SongDo
            {
                Title = songParams.SongTitle!,
                Isrc = StringUtils.NullIfEmpty(songParams.Isrc)
            };
            
            songRepository.Add(song);
        }
        
        return song;
    }
    
    /// <summary>
    /// Either creates a new <see cref="SongVariantDo"/> or returns the stored object for the <see cref="SongVariantParametersDto.SongVariantId"/>
    /// </summary>
    /// <param name="songVariantParams"></param>
    /// <returns></returns>
    /// <exception cref="SongVariantNotFoundException">if a Song variant ID was passed, but the variant does not exist</exception>
    private async Task<SongVariantDo> GetOrAddFromParameters(SongVariantParametersDto songVariantParams)
    {
        SongVariantDo songVariant;
        if (songVariantParams.SongVariantId is > 0)
        {
            logger.LogDebug("Checking if song variant exists: {songVariantId}", songVariantParams.SongVariantId);
            songVariant = await songVariantRepository.GetByPrimaryKeyAsync(songVariantParams.SongVariantId ?? 0) ?? throw new SongVariantNotFoundException(songVariantParams.SongVariantId ?? 0);
        }
        else
        {
            logger.LogDebug("Create a new song variant");
            songVariant = new SongVariantDo
            {
                SongId = songVariantParams.SongId ?? 0,
                VariantName = StringUtils.NullIfEmpty(songVariantParams.VariantName),
                Description = StringUtils.NullIfEmpty(songVariantParams.Description),
                IsrcOverride = StringUtils.NullIfEmpty(songVariantParams.IsrcOverride)
            };
            
            songVariantRepository.Add(songVariant);
        }
        
        return songVariant;
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
        var concertId = setlist.ConcertId;
        setlistRepository.Delete(setlist);
        await setlistRepository.SaveChangesAsync();
        
        await UpdateSetlistCacheForConcert(concertId, DateTimeOffset.Now);
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
        var concertId = setlistEntry.Setlist.ConcertId;
        setlistEntryRepository.Delete(setlistEntry);
        await setlistEntryRepository.SaveChangesAsync();
        
        await UpdateSetlistCacheForConcert(concertId, DateTimeOffset.Now);
    }

    /// <summary>
    /// Reorders the entries of a setlist
    /// </summary>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="orderedEntryIds">new order of all entries. ALL entries must be passed here!</param>
    /// <returns>the newly ordered (and saved) entries</returns>
    /// <exception cref="SetlistNotFoundException">If the setlist <paramref name="setlistId"/> does not exist</exception>
    /// <exception cref="InvalidEntryOrderException">If the new order could not be applied</exception>
    public async Task<List<SetlistEntryDo>> ReorderSetlistEntriesAsync(uint setlistId, string[] orderedEntryIds)
    {
        logger.LogDebug("Load setlist: {setlistId}", setlistId);
        var setlist = await setlistRepository.GetByPrimaryKeyAsync(setlistId) ?? throw new SetlistNotFoundException(setlistId);
        var entries = setlist
            .Entries
            .OrderBy(e => e.SortNumber)
            .ThenBy(e => e.SongNumber)
            .ToList();
        if (entries.Count != orderedEntryIds.Length)
        {
            logger.LogError("Could not reorder entries, because the number of given IDs did not match the number of entries.");
            throw new InvalidEntryOrderException("Number of ID does not match the number of entries.");
        }
        
        logger.LogDebug("Reorder {count} entries...", entries.Count);

        for (var i = 0u; i < orderedEntryIds.Length; i++)
        {
            var entryId = orderedEntryIds[i];
            var entry = entries.Find(e => e.Id == entryId) ?? throw new InvalidEntryOrderException($"The setlist entry with ID '{entryId}' did not exist.");
            var oldSortNumber = entry.SortNumber;
            entry.SortNumber = (i + 1) * 10;
            logger.LogDebug("Apply new position for entry with ID '{entryId}'. From {old} to {new}", entry.Id, oldSortNumber, entry.SortNumber);
            setlistEntryRepository.Update(entry);
        }
        
        logger.LogDebug("Reordered {count} entries!", entries.Count);
        await setlistEntryRepository.SaveChangesAsync();
        
        await UpdateSetlistCacheForConcert(setlist.ConcertId, DateTimeOffset.Now);

        return entries
            .OrderBy(entry => entry.SortNumber)
            .ThenBy(entry => entry.SongNumber)
            .ToList();
    }

    /// <summary>
    /// Writes the setlist into the Concerts database to cache it. All concerts with this setlist will be updated.
    /// </summary>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="cacheDate"></param>
    public async Task UpdateSetlistCacheForSetlist(uint setlistId, DateTimeOffset cacheDate)
    {
        var setlist = await setlistRepository.GetByPrimaryKeyAsync(setlistId);
        if (setlist != null)
        {
            await UpdateSetlistCacheForConcert(setlist.ConcertId, DateTimeOffset.Now);
        }
        else
        {
            logger.LogWarning("Could not find set list {setlistId}, while updating its cache!", setlistId);
        }
    }

    /// <summary>
    /// Writes the setlist into the Concerts database to cache it
    /// </summary>
    /// <param name="concertId">ID of the concert. Use <see cref="SetlistDto.ConcertId"/> of the setlist that has been changed.</param>
    /// <param name="cacheDate"></param>
    public async Task UpdateSetlistCacheForConcert(string concertId, DateTimeOffset cacheDate)
    {
        try
        {
            logger.LogDebug("Cache setlists for concert: {concertId}", concertId);

            var concert = await concertRepository.GetByIdAsync(concertId);
            if (concert == null)
            {
                logger.LogWarning("Setlist could not be cached because the concert '{concertId}' was not found.",
                    concertId);
                return;
            }

            logger.LogDebug("Load setlists for this concert...");
            var setlists = await setlistRepository
                .GetByConcertIdAsync(concertId)
                .Select(DtoMapper.ToDto)
                .ToListAsync();

            var setlistJson = JsonSerializer.Serialize(setlists, SetlistDtoJsonContext.Default.ListSetlistDto);
            concert.CachedSetlistsJson = setlistJson;
            concert.CachedSetlistsAt = cacheDate;

            await concertRepository.SaveAsync(concert);
            await setlistRepository.SaveChangesAsync();
            logger.LogDebug("Setlist cached");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Could not update setlist cache for concert '{concertId}'!", concertId);
        }
    }
}