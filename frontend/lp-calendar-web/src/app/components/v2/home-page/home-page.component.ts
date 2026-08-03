import { Component, inject, OnInit } from '@angular/core';
import { MatomoTracker } from 'ngx-matomo-client';
import { AuthService } from '../../../auth/auth.service';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { ConcertDto } from '../../../modules/lpshows-api';
import { ConcertsService } from '../../../services/concerts.service';
import { environment } from '../../../../environments/environment';
import { Message } from 'primeng/message';
import { Button } from 'primeng/button';
import { Tag } from 'primeng/tag';
import { Dialog } from 'primeng/dialog';
import { SplitButton } from 'primeng/splitbutton';
import { MenuItem, MessageService } from 'primeng/api';
import { Tooltip } from 'primeng/tooltip';
import { NgOptimizedImage } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ConcertTitleGenerator } from '../../../data/concert-title-generator';
import { downloadConcertIcs } from '../../../data/calendar-event';
import { ConcertBookmarkUpdateRequestDto } from '../../../modules/lpshows-api';
import { CalendarFeedBuilderComponent } from '../calendar-feed-builder/calendar-feed-builder.component';
import { ConcertScheduleComponent } from '../concert-schedule/concert-schedule.component';
import { TourMapTileComponent } from '../tour-map-tile/tour-map-tile.component';
import { DateTime } from 'luxon';

@Component({
  selector: 'app-home-page',
  imports: [
    Message,
    Button,
    Tag,
    Dialog,
    SplitButton,
    Tooltip,
    NgOptimizedImage,
    RouterLink,
    CalendarFeedBuilderComponent,
    ConcertScheduleComponent,
    TourMapTileComponent,
  ],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.css',
})
export class HomePageComponent implements OnInit {
  protected readonly environment = environment;
  protected readonly ConcertTitleGenerator = ConcertTitleGenerator;

  private readonly tracker = inject(MatomoTracker);
  private readonly authService = inject(AuthService);
  private readonly oidcSecurityService = inject(OidcSecurityService);
  private readonly messageService = inject(MessageService);

  nextConcert: ConcertDto | null = null;
  nextAttendingConcert: ConcertDto | null = null;
  nextBookmarkedConcert: ConcertDto | null = null;

  allConcerts: ConcertDto[] = [];

  attendingCount: number = 0;
  bookmarkedCount: number = 0;

  isLoadingNextConcert: boolean = false;
  isLoadingAttendingConcert: boolean = false;
  isLoadingBookmarkedConcert: boolean = false;

  private attendingIds = new Set<string>();
  private bookmarkedIds = new Set<string>();

  newFeatureAlertDismissedToken: string | null = localStorage.getItem(
    'alert.new-feature.dismissed-token',
  );
  currentNewFeatureAlertToken: string = '739e7808-da76-48c8-9c21-a1e13915e7fd';
  isLoggedIn$: boolean = false;
  iCalFeedUrl$: string = '';
  iCalButtonItems$: MenuItem[] = [];
  feedDialogVisible: boolean = false;
  nearestConcert: ConcertDto | null = null;
  isLocating: boolean = false;
  locationError: string | null = null;

  constructor(private concertsService: ConcertsService) {}

  ngOnInit() {
    this.loadNextConcert();
    this.loadAllConcerts();

    this.authService.isAuthenticated$.subscribe((isAuthenticated) => {
      console.debug('Home component is authenticated:', isAuthenticated);
      this.isLoggedIn$ = isAuthenticated;

      if (isAuthenticated) {
        console.debug("Load user's bookmarks etc.");
        this.loadNextBookmarkedConcert();
        this.loadNextAttendingConcert();
      }
    });

    this.iCalButtonItems$ = [
      {
        label: 'Subscribe',
        icon: 'pi pi-calendar-plus',
        command: () => {
          this.subscribeBtnClicked();
        },
      },
      {
        label: 'Copy link',
        icon: 'pi pi-copy',
        command: (evt) => {
          this.copyFeedUrlBtnClicked();
        },
      },
    ];
  }

  login(): void {
    this.oidcSecurityService.authorize();
  }

  openFeedDialog(): void {
    this.feedDialogVisible = true;
  }

  get hasPlottableConcerts(): boolean {
    return this.allConcerts.some(
      (concert) =>
        concert.venueLatitude != undefined &&
        concert.venueLatitude != 0 &&
        concert.venueLongitude != undefined &&
        concert.venueLongitude != 0,
    );
  }

  get featuredIsPast(): boolean {
    return this.featuredConcert?.isPast === true;
  }

  get countdownLabel(): string | null {
    const concert = this.featuredConcert;
    if (concert == null || this.featuredIsPast) {
      return null;
    }

    const target =
      concert.doorsTime ?? concert.mainStageTime ?? concert.postedStartTime;
    if (!target) {
      return null;
    }

    const diff = DateTime.fromISO(target).diffNow(['days', 'hours', 'minutes']);
    if (diff.toMillis() <= 0) {
      return null;
    }

    const days = Math.floor(diff.days);
    const hours = Math.floor(diff.hours);

    if (days > 0) {
      return `${days}d ${String(hours).padStart(2, '0')}h`;
    }

    return `${hours}h ${String(Math.floor(diff.minutes)).padStart(2, '0')}m`;
  }

  get countdownCaption(): string {
    return this.featuredConcert?.doorsTime ? 'until doors' : 'until showtime';
  }

  get featuredIsAttending(): boolean {
    const id = this.featuredConcert?.id;
    return id != undefined && this.attendingIds.has(id);
  }

  toggleFeaturedAttending(): void {
    const concert = this.featuredConcert;
    if (concert?.id == undefined) {
      return;
    }

    const concertId = concert.id;
    const wasAttending = this.featuredIsAttending;
    const status = wasAttending
      ? ConcertBookmarkUpdateRequestDto.StatusEnum.None
      : ConcertBookmarkUpdateRequestDto.StatusEnum.Attending;

    this.concertsService.setBookmarksForConcert(concertId, status).subscribe({
      next: () => {
        if (wasAttending) {
          this.attendingIds.delete(concertId);
          this.attendingCount = Math.max(0, this.attendingCount - 1);
        } else {
          this.attendingIds.add(concertId);
          this.attendingCount += 1;
        }
      },
      error: (err) => {
        console.error('Could not update attendance', err);
        this.messageService.add({
          severity: 'error',
          summary: 'Could not update your attendance',
          detail: 'Please try again in a moment.',
        });
      },
    });
  }

  get featuredIsBookmarked(): boolean {
    const id = this.featuredConcert?.id;
    return id != undefined && this.bookmarkedIds.has(id);
  }

  addFeaturedToCalendar(): void {
    const concert = this.featuredConcert;
    if (concert == null) {
      return;
    }

    if (!downloadConcertIcs(concert)) {
      this.messageService.add({
        severity: 'warn',
        summary: 'No start time for this show yet',
        detail:
          "Once the times are confirmed you'll be able to add it to your calendar.",
      });
      return;
    }

    this.tracker.trackEvent('ical_sub', 'single show', concert.id ?? '');
  }

  toggleFeaturedBookmark(): void {
    const concert = this.featuredConcert;
    if (concert?.id == undefined) {
      return;
    }

    const concertId = concert.id;
    const wasBookmarked = this.featuredIsBookmarked;
    const status = wasBookmarked
      ? ConcertBookmarkUpdateRequestDto.StatusEnum.None
      : ConcertBookmarkUpdateRequestDto.StatusEnum.Bookmarked;

    this.concertsService.setBookmarksForConcert(concertId, status).subscribe({
      next: () => {
        if (wasBookmarked) {
          this.bookmarkedIds.delete(concertId);
          this.bookmarkedCount = Math.max(0, this.bookmarkedCount - 1);
        } else {
          this.bookmarkedIds.add(concertId);
          this.bookmarkedCount += 1;
        }
      },
      error: (err) => {
        console.error('Could not update the bookmark', err);
        this.messageService.add({
          severity: 'error',
          summary: 'Could not update your bookmark',
          detail: 'Please try again in a moment.',
        });
      },
    });
  }

  findNearestConcert(): void {
    if (!navigator.geolocation) {
      this.locationError = "Your browser can't share a location";
      return;
    }

    this.isLocating = true;
    this.locationError = null;

    navigator.geolocation.getCurrentPosition(
      (position) => {
        this.isLocating = false;
        this.selectNearest(position.coords.latitude, position.coords.longitude);
      },
      (error) => {
        this.isLocating = false;
        this.locationError =
          error.code === error.PERMISSION_DENIED
            ? 'Location access was declined'
            : "Couldn't work out where you are";
      },
      { timeout: 10000, maximumAge: 600000 },
    );
  }

  private selectNearest(latitude: number, longitude: number): void {
    const candidates = this.allConcerts.filter(
      (concert) =>
        !concert.isPast &&
        concert.venueLatitude != undefined &&
        concert.venueLatitude != 0 &&
        concert.venueLongitude != undefined &&
        concert.venueLongitude != 0,
    );

    if (candidates.length === 0) {
      this.locationError = 'No upcoming shows to compare against';
      return;
    }

    let closest = candidates[0];
    let closestDistance = Number.POSITIVE_INFINITY;

    for (const concert of candidates) {
      const distance = haversineKm(
        latitude,
        longitude,
        concert.venueLatitude!,
        concert.venueLongitude!,
      );
      if (distance < closestDistance) {
        closestDistance = distance;
        closest = concert;
      }
    }

    this.nearestConcert = closest;
  }

  get featuredConcert(): ConcertDto | null {
    if (this.nextConcert != null) {
      return this.nextConcert;
    }

    return this.mostRecentConcert;
  }

  get featuredLabel(): string {
    return this.nextConcert != null ? 'Next show' : 'Most recent show';
  }

  get featuredDay(): string {
    const dateTime = this.featuredDateTime;
    return dateTime ? dateTime.toFormat('dd') : '';
  }

  get featuredMonth(): string {
    return this.featuredDateTime?.toFormat('LLL') ?? '';
  }

  get featuredYear(): string {
    return this.featuredDateTime?.toFormat('yyyy') ?? '';
  }

  private get featuredDateTime(): DateTime | null {
    const concert = this.featuredConcert;
    const start = concert?.mainStageTime ?? concert?.postedStartTime;
    if (!start) {
      return null;
    }

    return concert?.timeZoneId
      ? DateTime.fromISO(start, { zone: concert.timeZoneId })
      : DateTime.fromISO(start);
  }

  private get mostRecentConcert(): ConcertDto | null {
    const past = this.allConcerts
      .filter((concert) => concert.isPast && concert.postedStartTime)
      .sort((a, b) =>
        (b.postedStartTime ?? '').localeCompare(a.postedStartTime ?? ''),
      );

    return past.at(0) ?? null;
  }

  locationLabel(concert: ConcertDto): string {
    return [concert.city, concert.state, concert.country]
      .filter((part): part is string => (part?.length ?? 0) > 0)
      .join(', ');
  }

  onNewFeatureAlertClosed() {
    this.newFeatureAlertDismissedToken = this.currentNewFeatureAlertToken;
    localStorage.setItem(
      'alert.new-feature.dismissed-token',
      this.currentNewFeatureAlertToken,
    );
  }

  private loadNextConcert() {
    this.isLoadingNextConcert = true;
    this.concertsService.getNextConcert().subscribe({
      next: (result) => {
        this.nextConcert = result;
        this.isLoadingNextConcert = false;
      },
      error: (err) => {
        console.warn(
          'Next concert was not found. Maybe there is nothing scheduled.',
        );
        this.nextConcert = null;
        this.isLoadingNextConcert = false;
      },
    });
  }

  private loadAllConcerts() {
    this.concertsService.getFilteredConcerts(null, true).subscribe({
      next: (result) => {
        this.allConcerts = result;
      },
      error: (err) => {
        console.error('Could not load the concert list for the map', err);
        this.allConcerts = [];
      },
    });
  }


  private loadNextBookmarkedConcert() {
    this.isLoadingBookmarkedConcert = true;
    this.concertsService.getNextBookmarked().subscribe({
      next: (result) => {
        let next = result.at(0);
        if (next != undefined) {
          this.nextBookmarkedConcert = next;
        }

        this.bookmarkedCount = result.length;
        this.bookmarkedIds = new Set(
          result
            .map((concert) => concert.id)
            .filter((id): id is string => id != undefined),
        );

        this.isLoadingBookmarkedConcert = false;
      },
      error: (err) => {
        // If the request times out, an error will have been emitted.
        console.log(err);
        console.error('Next bookmarked concert could not be loaded');
        this.nextBookmarkedConcert = null;
        this.isLoadingBookmarkedConcert = false;
      },
    });
  }

  private loadNextAttendingConcert() {
    this.isLoadingAttendingConcert = true;
    this.concertsService.getNextAttending().subscribe({
      next: (result) => {
        let next = result.at(0);
        if (next != undefined) {
          this.nextAttendingConcert = next;
        }

        this.attendingCount = result.length;
        this.attendingIds = new Set(
          result
            .map((concert) => concert.id)
            .filter((id): id is string => id != undefined),
        );

        this.isLoadingAttendingConcert = false;
      },
      error: (err) => {
        // If the request times out, an error will have been emitted.
        console.log(err);
        console.error('Next concert you attend could not be loaded');
        this.nextAttendingConcert = null;
        this.isLoadingAttendingConcert = false;
      },
    });
  }

  private getCalFeedUrl() {
    let calendarUrl = this.iCalFeedUrl$;
    return calendarUrl.replace('https', 'webcal');
  }

  subscribeCustomBtnClicked() {
    // The builder may not have emitted a URL yet — fall back to the plain feed.
    if (this.iCalFeedUrl$.length === 0) {
      this.subscribeBtnClicked();
      return;
    }

    let calendarUrl = this.getCalFeedUrl();
    this.tracker.trackEvent('ical_sub', 'subscribed direct', calendarUrl);
    window.open(calendarUrl);
  }

  subscribeBtnClicked() {
    let calendarUrl = environment.apiBaseUrlLatest + '/feed/ical';
    calendarUrl = calendarUrl.replace('https', 'webcal');
    window.open(calendarUrl);
  }

  copyFeedUrlBtnClicked() {
    let calendarUrl = this.getCalFeedUrl();
    this.tracker.trackEvent('ical_sub', 'copied link', calendarUrl);
    navigator.clipboard.writeText(calendarUrl).then((_) => {
      console.debug('copied iCal URL: ' + calendarUrl);
      this.messageService.add({
        severity: 'success',
        summary: 'Copied URL to clipboard!',
      });
    });
  }

  onFeedUrlUpdated(newUrl: string) {
    console.log('New URL: ' + newUrl);
    this.iCalFeedUrl$ = newUrl;
  }
}

function haversineKm(
  lat1: number,
  lon1: number,
  lat2: number,
  lon2: number,
): number {
  const earthRadiusKm = 6371;
  const toRadians = (degrees: number) => (degrees * Math.PI) / 180;

  const deltaLat = toRadians(lat2 - lat1);
  const deltaLon = toRadians(lon2 - lon1);

  const a =
    Math.sin(deltaLat / 2) ** 2 +
    Math.cos(toRadians(lat1)) *
      Math.cos(toRadians(lat2)) *
      Math.sin(deltaLon / 2) ** 2;

  return earthRadiusKm * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}
