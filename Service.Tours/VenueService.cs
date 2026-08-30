using Database.Tours.DataObjects;
using Database.Tours.Extensions;
using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.Extensions.Logging;
using Service.Tours.Exceptions;

namespace Service.Tours;

/// <summary>
/// Service to manage venues and their names
/// </summary>
public class VenueService(IVenueRepository venueRepository, ILogger<VenueService> logger)
{
    /// <summary>
    /// Creates a new venue
    /// </summary>
    /// <param name="request"></param>
    /// <returns>ID of the created venue</returns>
    public async Task<uint> CreateVenueAsync(CreateVenueRequestDto request)
    {
        logger.LogDebug("Requested to create a new venue: {name}", request.CurrentName);
        var venue = request.ToDo();
        venue.PreviousNames.Add(new PreviousVenueNameDo
        {
            Name = venue.CurrentName,
            Venue = venue,
            From = DateOnly.FromDateTime(DateTime.Now),
        });
        venueRepository.Add(venue);
        await venueRepository.SaveChangesAsync();
        logger.LogDebug("Saved venue in DB. New ID: {venueID}", venue.Id);
        return venue.Id;
    }

    /// <summary>
    /// Returns the basic information about a venue by its ID
    /// </summary>
    /// <param name="venueId">ID of the venue</param>
    /// <returns></returns>
    /// <exception cref="VenueNotFoundException">if the venue was not found</exception>
    public async Task<VenueBo> GetVenueByIdAsync(uint venueId)
    {
        logger.LogDebug("Searching for venue with ID: {venueId}", venueId);
        var venue = await venueRepository.GetByPrimaryKeyWithoutReferencesAsync(venueId) ?? throw new VenueNotFoundException(venueId);
        logger.LogDebug("Found venue: {name}", venue.CurrentName);
        return venue.ToDto();
    }
    
    /// <summary>
    /// Returns the information about a venue by its ID including the data of the city, state and country.
    /// </summary>
    /// <param name="venueId">ID of the venue</param>
    /// <returns></returns>
    /// <exception cref="VenueNotFoundException">if the venue was not found</exception>
    public async Task<VenueWithDetailsBo> GetVenueWithDetailsByIdAsync(uint venueId)
    {
        logger.LogDebug("Searching for venue details with ID: {venueId}", venueId);
        var venue = await venueRepository.GetByPrimaryKeyAsync(venueId) ?? throw new VenueNotFoundException(venueId);
        logger.LogDebug("Found venue: {name}", venue.CurrentName);
        return venue.ToDtoWithAllDetails();
    }

    /// <summary>
    /// Returns a list of all venues
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the search</param>
    /// <returns>List of all venues</returns>
    public IAsyncEnumerable<VenueBo> GetAllVenuesAsync(CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Searching for all venues...");
        return venueRepository
            .QueryAsync(cancellationToken)
            .Select(DtoMapper.ToDto);
    }

    /// <summary>
    /// Updates the information about a venue.
    /// </summary>
    /// <remarks>
    /// Note that this method cannot update the name of the venue.
    /// Please use <see cref="AddVenueNameAsync"/> or <see cref="UpdateVenueNameAsync"/> instead.
    /// </remarks>
    /// <param name="request">New data of the venue. Partial updates are not possible!</param>
    /// <param name="venueId">ID of the venue to update</param>
    /// <exception cref="VenueNotFoundException">if the venue does not exist</exception>
    public async Task UpdateVenueAsync(UpdateVenueRequestDto request, uint venueId)
    {
        logger.LogDebug("Updating venue with ID: {venueId}", venueId);
        var venue = await venueRepository.GetByPrimaryKeyWithoutReferencesAsync(venueId) ?? throw new VenueNotFoundException(venueId);
        logger.LogDebug("Found venue: {name}", venue.CurrentName);
        venue.UpdateFromRequestDto(request);
        venueRepository.Update(venue);
        await venueRepository.SaveChangesAsync();
        logger.LogDebug("Successfully updated the venue.");
    }

    /// <summary>
    /// Deletes a venue with a given ID
    /// </summary>
    /// <param name="venueId">ID of the venue</param>
    /// <exception cref="VenueNotFoundException">if the venue does not exist</exception>
    public async Task DeleteVenueAsync(uint venueId)
    {
        logger.LogDebug("Deleting venue with ID: {venueId}", venueId);
        var venue = await venueRepository.GetByPrimaryKeyWithoutReferencesAsync(venueId) ?? throw new VenueNotFoundException(venueId);
        venueRepository.Delete(venue);
        await venueRepository.SaveChangesAsync();
        logger.LogDebug("Deleted venue with ID: {venueId}", venueId);
    }

    /// <summary>
    /// Adds a name to a venue. This can either be the new name or some historic name
    /// </summary>
    /// <param name="request"></param>
    /// <exception cref="VenueNotFoundException"></exception>
    public async Task AddVenueNameAsync(AddVenueNameRequestDto request, uint venueId)
    {
        logger.LogDebug("Adding name '{newName}' for venue with ID: {venueId}", request.Name, venueId);
        var venue = await venueRepository.GetByPrimaryKeyAsync(venueId);
        if (venue == null)
        {
            logger.LogError("Failed to add venue name! Could not find venue with ID: {venueId}", venueId);
            throw new VenueNotFoundException(venueId);
        }

        var newName = new PreviousVenueNameDo
        {
            VenueId = venue.Id,
            Venue = venue,
            Name = request.Name,
            From = request.From,
            To = request.To,
        };
        venue.PreviousNames.Add(newName);
        logger.LogDebug("Added new name to PreviousNames: {newName} (From: {from}; To: {to}", newName.Name, newName.From, newName.To);
        
        // fix the time ranges of the entries if possible
        ValidateVenueNameDateRanges(venue);
        
        // update to the currently valid name
        venue.CurrentName = venue.PreviousNames.GetValidNameEntryAt(DateTimeOffset.UtcNow).Name;
        
        venueRepository.Update(venue);
        await venueRepository.SaveChangesAsync();
        logger.LogDebug("Successfully added new name to venue.");
    }
    
    /// <summary>
    /// Updates a name of a venue
    /// </summary>
    /// <param name="request"></param>
    /// <exception cref="VenueNotFoundException"></exception>
    public async Task UpdateVenueNameAsync(UpdateVenueNameRequestDto request, uint venueId, uint venueNameId)
    {
        logger.LogDebug("Updating the name '{venueNameId}' to '{newName}' for venue with ID: {venueId}", venueNameId, request.Name, venueId);
        var venue = await venueRepository.GetByPrimaryKeyAsync(venueId);
        if (venue == null)
        {
            logger.LogError("Failed to update venue name! Could not find venue with ID: {venueId}", venueId);
            throw new VenueNotFoundException(venueId);
        }

        var nameToUpdate = venue.PreviousNames.FirstOrDefault(pn => pn.Id == venueNameId) ?? throw new VenueNameNotFoundException(venueId, venueNameId);
        nameToUpdate.Name = request.Name;
        nameToUpdate.From = request.From;
        nameToUpdate.To = request.To;
        
        // fix the time ranges of the entries if possible
        ValidateVenueNameDateRanges(venue);
        
        // update to the currently valid name
        venue.CurrentName = venue.PreviousNames.GetValidNameEntryAt(DateTimeOffset.UtcNow).Name;
        
        venueRepository.Update(venue);
        await venueRepository.SaveChangesAsync();
        logger.LogDebug("Successfully updated new name of venue.");
    }
    
    /// <summary>
    /// Deletes a name of a venue
    /// </summary>
    /// <param name="venueId">ID of the venue</param>
    /// <param name="venueNameId">ID of the name</param>
    /// <exception cref="VenueNotFoundException"></exception>
    public async Task DeleteVenueNameAsync(uint venueId, uint venueNameId)
    {
        logger.LogDebug("Deleting the name '{venueNameId}' for venue with ID: {venueId}", venueNameId, venueId);
        var venue = await venueRepository.GetByPrimaryKeyAsync(venueId);
        if (venue == null)
        {
            logger.LogError("Failed to delete venue name! Could not find venue with ID: {venueId}", venueId);
            throw new VenueNotFoundException(venueId);
        }

        venue.PreviousNames = venue
            .PreviousNames
            .Where(pn => pn.Id != venueNameId)
            .ToArray();
        
        // fix the time ranges of the entries if possible
        ValidateVenueNameDateRanges(venue);
        
        // update to the currently valid name
        venue.CurrentName = venue.PreviousNames.GetValidNameEntryAt(DateTimeOffset.UtcNow).Name;
        
        venueRepository.Update(venue);
        await venueRepository.SaveChangesAsync();
        logger.LogDebug("Successfully deleted name of venue.");
    }

    /// <summary>
    /// Makes sure that the <see cref="PreviousVenueNameDo"/>s are not overlapping.
    /// If it can't be fixed automatically, an exception will be thrown.
    /// </summary>
    /// <param name="venue"></param>
    private void ValidateVenueNameDateRanges(VenueDo venue)
    {
        logger.LogDebug("Validating venue name date ranges for venue '{currentName}' with ID: {venueId}", venue.CurrentName, venue.Id);
        var names = venue
            .PreviousNames
            .OrderBy(pn => pn.From)
            .ToArray();

        for (var i = 0; i < names.Length; i++)
        {
            var currentEntry = names[i];
            logger.LogDebug("Checking entry: {entry}", currentEntry);
            
            // if previous entry exists, make sure it ends the day before the current entry
            if (i > 0)
            {
                var previousEntry = names[i - 1];
                logger.LogDebug("Checking against previousEntry: {previousEntry}", previousEntry);

                var expectedPreviousEnd = currentEntry.From.AddDays(-1);
                logger.LogDebug("Previous entry should end at: {expectedPreviousEnd}", expectedPreviousEnd);

                // check if there is a gap
                if (previousEntry.To == null)
                {
                    logger.LogInformation("Previous name '{previousName}' had no end date. Will set it to the day before the start of the current entry: {currentFrom}", previousEntry.Name, currentEntry.From);
                    previousEntry.To = expectedPreviousEnd;
                } else if (expectedPreviousEnd > previousEntry.To)
                {
                    logger.LogWarning("There seems to be a gap between '{previousName}' (until {previousTo}) and '{followingName}' (from {currentFrom}). This can't be fixed automatically!", previousEntry.Name, previousEntry.To, currentEntry.Name, currentEntry.From);
                } else if (expectedPreviousEnd < previousEntry.To)
                {
                    logger.LogInformation("The name '{previousName}' seems to overlap with '{followingName}'! The previous entry will be cut to respect the start date of the following entry\\n{previousEntry}\n\n{followingEntry}", previousEntry.Name, currentEntry.Name, previousEntry, currentEntry);
                    
                    // set the previous entry to one day before the current one, which is the following name
                    previousEntry.To = currentEntry.From.AddDays(-1);
                }
            }
            else
            {
                logger.LogTrace("This is the first entry, so there won't be a check against any previous entries.");
            }
        }
    }
}