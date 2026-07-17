using Database.Tours.DataObjects;
using Database.Tours.Extensions;
using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.Extensions.Logging;
using Mysqlx;
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
    public async Task<VenueDto> GetVenueByIdAsync(uint venueId)
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
    public async Task<VenueDto> GetVenueWithDetailsByIdAsync(uint venueId)
    {
        logger.LogDebug("Searching for venue details with ID: {venueId}", venueId);
        var venue = await venueRepository.GetByPrimaryKeyAsync(venueId) ?? throw new VenueNotFoundException(venueId);
        logger.LogDebug("Found venue: {name}", venue.CurrentName);
        return venue.ToDtoWithCityDetails();
    }

    /// <summary>
    /// Adds a name to a venue. This can either be the new name or some historic name
    /// </summary>
    /// <param name="request"></param>
    /// <exception cref="VenueNotFoundException"></exception>
    public async Task AddVenueName(AddVenueNameRequestDto request)
    {
        logger.LogDebug("Adding name '{newName}' for venue with ID: {venueId}", request.Name, request.VenueId);
        var venue = await venueRepository.GetByPrimaryKeyAsync(request.VenueId);
        if (venue == null)
        {
            logger.LogError("Failed to add venue name! Could not find venue with ID: {venueId}", request.VenueId);
            throw new VenueNotFoundException(request.VenueId);
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