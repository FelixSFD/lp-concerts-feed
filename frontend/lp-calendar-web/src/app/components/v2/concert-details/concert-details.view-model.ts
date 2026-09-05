import {DateTime} from 'luxon';
import {
  AdjacentConcertsResponseDto,
  ConcertDto,
  ConcertStatusValueDto,
  ConcertWithSetlistsDto,
  GetConcertBookmarkCountsResponseDto
} from '../../../modules/lpshows-api';
import {ConcertDetailsDto} from '../../../modules/lpshows-api/v3';
import {Setlist} from '../../../data/setlists/setlist';
import {ConcertTitleGenerator} from '../../../data/concert-title-generator';
import {environment} from '../../../../environments/environment';

export interface ConcertCoordinates {
  latitude: number;
  longitude: number;
}

export interface ConcertBookmarksViewModel {
  bookmarkedCount: number;
  attendingCount: number;
  currentUserStatus: GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum;
  loading: boolean;
}

export interface ConcertBadgesData {
  concertStatus?: ConcertStatusValueDto;
  showType?: string;
  mainStageTime?: DateTime;
  lpuEarlyEntryConfirmed?: boolean;
  lpuEarlyEntryTime?: DateTime;
}

export interface ConcertTimelineStepViewModel {
  label: string;
  time?: DateTime;
  hasTime: boolean;
  markerIcon: string;
  isLead?: boolean;
}

export interface ConcertTimelineViewModel {
  isPast: boolean;
  earlyEntry: ConcertTimelineStepViewModel;
  doors: ConcertTimelineStepViewModel;
  stage: ConcertTimelineStepViewModel;
}

export class ConcertDetailsViewModel {
  id: string = '';
  title: string = '';
  tourName?: string;
  locationLabel: string = '';
  postedStartTime?: DateTime;
  isPast: boolean = false;
  countdownTargetTime?: DateTime;

  // Navigation
  previousConcertId?: string;
  nextConcertId?: string;

  // Wiki / Linkinpedia
  wikiUrl?: string;

  // Timeline
  timeline!: ConcertTimelineViewModel;
  expectedSetDuration?: number;

  // Badges
  badges?: ConcertBadgesData;

  // Bookmarks / User Status
  bookmarks?: ConcertBookmarksViewModel;

  // Setlists
  setlists: Setlist[] = [];
  setlistsCacheUpdatedAt?: DateTime | null;

  // Map & Schedule
  venueCoordinates?: ConcertCoordinates;
  venuePinTitle?: string;
  scheduleImageUrl?: string;

  public static fromLegacyDto(
    dto: ConcertDto | ConcertWithSetlistsDto,
    adjacent?: AdjacentConcertsResponseDto | null,
    bookmarks?: GetConcertBookmarkCountsResponseDto | null,
    bookmarksLoading: boolean = false,
    setlists?: Setlist[],
    setlistsCacheUpdatedAt?: DateTime | null
  ): ConcertDetailsViewModel {
    const vm = new ConcertDetailsViewModel();
    vm.id = dto.id ?? '';
    vm.title = ConcertTitleGenerator.getVenueTitleFor(dto);
    vm.tourName = dto.tourName ?? undefined;

    const locationParts: string[] = [];
    if (dto.city) {
      locationParts.push(dto.city);
    }
    if (dto.state && dto.state.length > 0) {
      locationParts.push(dto.state);
    }
    const cityState = locationParts.join(', ');
    vm.locationLabel = `${cityState ? cityState + ', ' : ''}${dto.country ?? ''}`;

    const timeZoneId = dto.timeZoneId ?? undefined;
    const startDt = ConcertDetailsViewModel.parseDateTime(dto.postedStartTime, timeZoneId);
    const mainStageDt = ConcertDetailsViewModel.parseDateTime(dto.mainStageTime, timeZoneId);
    const doorsDt = ConcertDetailsViewModel.parseDateTime(dto.doorsTime, timeZoneId);
    const earlyEntryDt = ConcertDetailsViewModel.parseDateTime(dto.lpuEarlyEntryTime, timeZoneId);

    vm.postedStartTime = startDt;
    vm.isPast = dto.isPast ?? false;
    vm.countdownTargetTime = mainStageDt ?? startDt ?? undefined;

    vm.previousConcertId = adjacent?.previous ?? undefined;
    vm.nextConcertId = adjacent?.next ?? undefined;
    vm.wikiUrl = ConcertDetailsViewModel.buildWikiUrl(startDt);

    vm.timeline = ConcertDetailsViewModel.buildTimeline(
      vm.isPast,
      startDt,
      mainStageDt,
      doorsDt,
      earlyEntryDt
    );

    vm.expectedSetDuration = (dto.expectedSetDuration && dto.expectedSetDuration > 0) ? dto.expectedSetDuration : undefined;

    vm.badges = {
      concertStatus: dto.concertStatus,
      showType: dto.showType,
      mainStageTime: mainStageDt,
      lpuEarlyEntryConfirmed: dto.lpuEarlyEntryConfirmed,
      lpuEarlyEntryTime: earlyEntryDt
    };

    vm.bookmarks = ConcertDetailsViewModel.buildBookmarks(bookmarks, bookmarksLoading);

    if (setlists) {
      vm.setlists = setlists;
    } else if ('cachedSetlists' in dto && dto.cachedSetlists) {
      vm.setlists = dto.cachedSetlists.map(s => Setlist.fromDto(s));
    } else {
      vm.setlists = [];
    }

    if (setlistsCacheUpdatedAt !== undefined) {
      vm.setlistsCacheUpdatedAt = setlistsCacheUpdatedAt;
    } else if ('cachedSetlistsAt' in dto && dto.cachedSetlistsAt) {
      vm.setlistsCacheUpdatedAt = DateTime.fromISO(dto.cachedSetlistsAt, {setZone: false});
    }

    const hasCoords = dto.venueLatitude !== undefined && dto.venueLongitude !== undefined &&
                      dto.venueLatitude !== 0 && dto.venueLongitude !== 0;
    if (hasCoords) {
      vm.venueCoordinates = {
        latitude: dto.venueLatitude!,
        longitude: dto.venueLongitude!
      };
      if (dto.venue) {
        vm.venuePinTitle = dto.city ? `${dto.venue}, ${dto.city}` : dto.venue;
      } else {
        vm.venuePinTitle = dto.city ?? undefined;
      }
    }

    if (dto.scheduleImageFile) {
      vm.scheduleImageUrl = `${environment.imageBaseUrl}/${dto.scheduleImageFile}`;
    }

    return vm;
  }

  public static fromV3Dto(
    dto: ConcertDetailsDto,
    adjacent?: AdjacentConcertsResponseDto | null,
    bookmarks?: GetConcertBookmarkCountsResponseDto | null,
    bookmarksLoading: boolean = false,
    setlists?: Setlist[],
    setlistsCacheUpdatedAt?: DateTime | null
  ): ConcertDetailsViewModel {
    const vm = new ConcertDetailsViewModel();
    vm.id = dto.id ?? '';
    const venueName = dto.venue?.currentName;
    const cityName = dto.venue?.city?.name;
    const stateName = dto.venue?.city?.state?.name;
    const countryName = dto.venue?.city?.country?.name;

    vm.title = dto.customTitle || venueName || (cityName ? `Concert in ${cityName}` : 'Concert Details');
    vm.tourName = dto.tour?.name ?? undefined;

    const locationParts: string[] = [];
    if (cityName) locationParts.push(cityName);
    if (stateName) locationParts.push(stateName);
    const cityState = locationParts.join(', ');
    vm.locationLabel = `${cityState ? cityState + ', ' : ''}${countryName ?? ''}`;

    const timeZoneId = dto.venue?.timeZoneId ?? undefined;
    const startDt = ConcertDetailsViewModel.parseDateTime(dto.postedStartTime, timeZoneId);
    const mainStageDt = ConcertDetailsViewModel.parseDateTime(dto.mainStageTime, timeZoneId);
    const doorsDt = ConcertDetailsViewModel.parseDateTime(dto.doorsTime, timeZoneId);
    const earlyEntryDt = ConcertDetailsViewModel.parseDateTime(dto.lpuEarlyEntryTime, timeZoneId);

    vm.postedStartTime = startDt;
    vm.isPast = startDt ? startDt < DateTime.now() : false;
    vm.countdownTargetTime = mainStageDt ?? startDt ?? undefined;

    vm.previousConcertId = adjacent?.previous ?? undefined;
    vm.nextConcertId = adjacent?.next ?? undefined;
    vm.wikiUrl = ConcertDetailsViewModel.buildWikiUrl(startDt);

    vm.timeline = ConcertDetailsViewModel.buildTimeline(
      vm.isPast,
      startDt,
      mainStageDt,
      doorsDt,
      earlyEntryDt
    );

    const durationNum = dto.expectedSetDurationMinutes ? parseInt(dto.expectedSetDurationMinutes, 10) : undefined;
    vm.expectedSetDuration = (durationNum && durationNum > 0) ? durationNum : undefined;

    vm.badges = {
      concertStatus: dto.status,
      showType: dto.concertType?.name,
      mainStageTime: mainStageDt,
      lpuEarlyEntryConfirmed: dto.lpuEarlyEntryConfirmed,
      lpuEarlyEntryTime: earlyEntryDt
    };

    vm.bookmarks = ConcertDetailsViewModel.buildBookmarks(bookmarks, bookmarksLoading);

    vm.setlists = setlists ?? [];
    vm.setlistsCacheUpdatedAt = setlistsCacheUpdatedAt ?? null;

    const lat = dto.venue?.latitude;
    const lon = dto.venue?.longitude;
    const hasCoords = lat != null && lon != null && lat !== 0 && lon !== 0;
    if (hasCoords) {
      vm.venueCoordinates = { latitude: lat!, longitude: lon! };
      if (venueName) {
        vm.venuePinTitle = cityName ? `${venueName}, ${cityName}` : venueName;
      } else {
        vm.venuePinTitle = cityName ?? undefined;
      }
    }

    if (dto.scheduleImageFile) {
      vm.scheduleImageUrl = `${environment.imageBaseUrl}/${dto.scheduleImageFile}`;
    }

    return vm;
  }

  private static parseDateTime(iso?: string, timeZoneId?: string): DateTime | undefined {
    if (!iso) return undefined;
    return timeZoneId ? DateTime.fromISO(iso, {zone: timeZoneId}) : DateTime.fromISO(iso);
  }

  private static buildWikiUrl(startDt?: DateTime): string | undefined {
    if (!startDt || !startDt.isValid) return undefined;
    return 'https://linkinpedia.com/wiki/Live:' + startDt.toFormat('yyyyMMdd');
  }

  private static buildBookmarks(
    bookmarks?: GetConcertBookmarkCountsResponseDto | null,
    loading: boolean = false
  ): ConcertBookmarksViewModel | undefined {
    if (!bookmarks && !loading) {
      return undefined;
    }
    return {
      bookmarkedCount: bookmarks?.bookmarked ?? 0,
      attendingCount: bookmarks?.attending ?? 0,
      currentUserStatus: bookmarks?.currentUserStatus ?? GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum.None,
      loading
    };
  }

  private static buildTimeline(
    isPast: boolean,
    startDt?: DateTime,
    mainStageDt?: DateTime,
    doorsDt?: DateTime,
    earlyEntryDt?: DateTime
  ): ConcertTimelineViewModel {
    const hasEarlyEntry = !!earlyEntryDt;
    const hasDoors = !!doorsDt;
    const stageDt = mainStageDt ?? startDt;

    return {
      isPast,
      earlyEntry: {
        label: 'LPU early entry',
        time: earlyEntryDt,
        hasTime: hasEarlyEntry,
        markerIcon: 'pi pi-star'
      },
      doors: {
        label: 'Doors open',
        time: doorsDt,
        hasTime: hasDoors,
        markerIcon: 'pi pi-sign-in'
      },
      stage: {
        label: mainStageDt ? 'LP stage time' : 'Show starts',
        time: stageDt,
        hasTime: !!stageDt,
        markerIcon: 'pi pi-microphone',
        isLead: true
      }
    };
  }
}
