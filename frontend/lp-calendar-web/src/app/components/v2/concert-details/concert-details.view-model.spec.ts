import {DateTime} from 'luxon';
import {ConcertDetailsViewModel} from './concert-details.view-model';
import {
  AdjacentConcertsResponseDto,
  ConcertDto,
  ConcertStatusValueDto,
  GetConcertBookmarkCountsResponseDto
} from '../../../modules/lpshows-api';
import {ConcertDetailsDto} from '../../../modules/lpshows-api/v3';

describe('ConcertDetailsViewModel', () => {
  describe('fromLegacyDto', () => {
    it('should map legacy ConcertDto fields properly', () => {
      const legacyDto: ConcertDto = {
        id: 'concert-1',
        venue: 'Kia Forum',
        city: 'Inglewood',
        state: 'CA',
        country: 'United States',
        tourName: 'From Zero World Tour',
        postedStartTime: '2026-09-11T19:30:00Z',
        timeZoneId: 'America/Los_Angeles',
        mainStageTime: '2026-09-11T20:45:00Z',
        doorsTime: '2026-09-11T18:00:00Z',
        lpuEarlyEntryTime: '2026-09-11T17:00:00Z',
        lpuEarlyEntryConfirmed: true,
        concertStatus: ConcertStatusValueDto.Planned,
        showType: 'Arena Show',
        expectedSetDuration: 120,
        venueLatitude: 33.9583,
        venueLongitude: -118.3419,
        scheduleImageFile: 'schedule-1.jpg',
        isPast: false
      };

      const adjacent: AdjacentConcertsResponseDto = {
        previous: 'concert-0',
        next: 'concert-2'
      };

      const bookmarks: GetConcertBookmarkCountsResponseDto = {
        bookmarked: 42,
        attending: 15,
        currentUserStatus: GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum.Bookmarked
      };

      const vm = ConcertDetailsViewModel.fromLegacyDto(legacyDto, adjacent, bookmarks, false);

      expect(vm.id).toBe('concert-1');
      expect(vm.title).toBe('Kia Forum');
      expect(vm.tourName).toBe('From Zero World Tour');
      expect(vm.locationLabel).toBe('Inglewood, CA, United States');
      expect(vm.isPast).toBeFalse();
      expect(vm.postedStartTime instanceof DateTime).toBeTrue();
      expect(vm.postedStartTime?.toISO()).toBe(DateTime.fromISO('2026-09-11T19:30:00Z', {zone: 'America/Los_Angeles'}).toISO());
      expect(vm.countdownTargetTime instanceof DateTime).toBeTrue();
      expect(vm.countdownTargetTime?.toISO()).toBe(DateTime.fromISO('2026-09-11T20:45:00Z', {zone: 'America/Los_Angeles'}).toISO());
      expect(vm.previousConcertId).toBe('concert-0');
      expect(vm.nextConcertId).toBe('concert-2');
      expect(vm.wikiUrl).toContain('https://linkinpedia.com/wiki/Live:20260911');
      expect(vm.expectedSetDuration).toBe(120);

      // Badges
      expect(vm.badges?.concertStatus).toBe(ConcertStatusValueDto.Planned);
      expect(vm.badges?.showType).toBe('Arena Show');
      expect(vm.badges?.mainStageTime instanceof DateTime).toBeTrue();
      expect(vm.badges?.lpuEarlyEntryConfirmed).toBeTrue();
      expect(vm.badges?.lpuEarlyEntryTime instanceof DateTime).toBeTrue();

      // Bookmarks
      expect(vm.bookmarks?.bookmarkedCount).toBe(42);
      expect(vm.bookmarks?.attendingCount).toBe(15);
      expect(vm.bookmarks?.currentUserStatus).toBe(GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum.Bookmarked);
      expect(vm.bookmarks?.loading).toBeFalse();

      // Timeline
      expect(vm.timeline.isPast).toBeFalse();
      expect(vm.timeline.earlyEntry.hasTime).toBeTrue();
      expect(vm.timeline.earlyEntry.time instanceof DateTime).toBeTrue();
      expect(vm.timeline.earlyEntry.time?.toISO()).toBe(DateTime.fromISO('2026-09-11T17:00:00Z', {zone: 'America/Los_Angeles'}).toISO());
      expect(vm.timeline.doors.hasTime).toBeTrue();
      expect(vm.timeline.doors.time instanceof DateTime).toBeTrue();
      expect(vm.timeline.stage.label).toBe('LP stage time');
      expect(vm.timeline.stage.time instanceof DateTime).toBeTrue();
      expect(vm.timeline.stage.time?.toISO()).toBe(DateTime.fromISO('2026-09-11T20:45:00Z', {zone: 'America/Los_Angeles'}).toISO());

      // Coordinates & Schedule
      expect(vm.venueCoordinates).toEqual({ latitude: 33.9583, longitude: -118.3419 });
      expect(vm.venuePinTitle).toBe('Kia Forum, Inglewood');
      expect(vm.scheduleImageUrl).toContain('schedule-1.jpg');
    });

    it('should handle missing optional fields gracefully', () => {
      const minimalDto: ConcertDto = {
        id: 'concert-min',
        city: 'London',
        country: 'United Kingdom',
        postedStartTime: '2026-06-20T19:00:00Z',
        venueLatitude: 0,
        venueLongitude: 0
      };

      const vm = ConcertDetailsViewModel.fromLegacyDto(minimalDto);

      expect(vm.id).toBe('concert-min');
      expect(vm.locationLabel).toBe('London, United Kingdom');
      expect(vm.venueCoordinates).toBeUndefined();
      expect(vm.scheduleImageUrl).toBeUndefined();
      expect(vm.timeline.earlyEntry.hasTime).toBeFalse();
      expect(vm.timeline.doors.hasTime).toBeFalse();
      expect(vm.timeline.stage.label).toBe('Show starts');
      expect(vm.timeline.stage.time instanceof DateTime).toBeTrue();
      expect(vm.timeline.stage.time?.toISO()).toBe(DateTime.fromISO('2026-06-20T19:00:00Z').toISO());
    });
  });

  describe('fromV3Dto', () => {
    it('should map v3 ConcertDetailsDto fields properly', () => {
      const v3Dto: ConcertDetailsDto = {
        id: 'v3-concert-1',
        concertType: { id: 1, name: 'Festival' },
        customTitle: 'Rock am Ring 2026',
        tour: { id: 'tour-1', name: 'Summer Festival Tour' },
        venue: {
          id: 'venue-1',
          currentName: 'Nürburgring',
          timeZoneId: 'Europe/Berlin',
          latitude: 50.334,
          longitude: 6.942,
          countryCode: 'DE',
          city: {
            id: 'city-1',
            name: 'Nürburg',
            nativeName: 'Nürburg',
            countryCode: 'DE',
            country: { isoCode: 'DE', name: 'Germany', nativeName: 'Deutschland' },
            state: null
          },
          venueNames: []
        },
        postedStartTime: '2026-06-05T21:00:00Z',
        expectedSetDurationMinutes: '90'
      };

      const vm = ConcertDetailsViewModel.fromV3Dto(v3Dto);

      expect(vm.id).toBe('v3-concert-1');
      expect(vm.title).toBe('Rock am Ring 2026');
      expect(vm.tourName).toBe('Summer Festival Tour');
      expect(vm.locationLabel).toBe('Nürburg, Germany');
      expect(vm.postedStartTime instanceof DateTime).toBeTrue();
      expect(vm.postedStartTime?.toISO()).toBe(DateTime.fromISO('2026-06-05T21:00:00Z', {zone: 'Europe/Berlin'}).toISO());
      expect(vm.expectedSetDuration).toBe(90);
      expect(vm.venueCoordinates).toEqual({ latitude: 50.334, longitude: 6.942 });
      expect(vm.venuePinTitle).toBe('Nürburgring, Nürburg');
      expect(vm.badges?.showType).toBe('Festival');
    });
  });
});
