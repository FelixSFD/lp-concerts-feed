import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {AuthService} from '../../../auth/auth.service';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import { environment } from "../../../../environments/environment";
import {Message} from 'primeng/message';
import {Card} from 'primeng/card';
import {Tag} from 'primeng/tag';
import {Button} from 'primeng/button';
import {MessageService} from 'primeng/api';
import {Router, RouterLink} from '@angular/router';
import {CalendarFeedBuilderComponent} from '../calendar-feed-builder/calendar-feed-builder.component';
import {ConcertCardComponent} from '../concert-card/concert-card.component';
import { ConcertsService } from '../../../services/concerts.service';
import { CalendarSubscriptionDto, CalendarSubscriptionSettingsDto, ConcertDetailsDto } from '../../../modules/lpshows-api/v3';
import { CalendarSubscriptionsService } from '../../../services/calendar-subscriptions.service';
import { subscriptionErrorMessage } from '../calendar-subscriptions/calendar-subscription-errors';
import { catchError, of, switchMap } from 'rxjs';
import { HeroCountdownComponent } from '../hero-countdown/hero-countdown.component';
import { DatePipe } from '@angular/common';

interface HeroImage {
  readonly src: string;
  readonly attribution?: {
    readonly text: string;
    readonly url?: string;
    readonly icon?: string;
  };
}

@Component({
  selector: 'app-home-page',
  imports: [
    Message,
    Card,
    Tag,
    Button,
    RouterLink,
    CalendarFeedBuilderComponent,
    ConcertCardComponent,
    HeroCountdownComponent,
    DatePipe,
  ],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.css',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class HomePageComponent implements OnInit {
  protected readonly environment = environment;

  // Attribution supports optional url and PrimeIcons icon (e.g. 'pi-instagram').
  // Keep this list in sync with the images in public/home-images.
  protected readonly heroImages: readonly HeroImage[] = [
    { src: '/home-images/Linkin-Park-show.webp', attribution: { text: 'Elina Shelest', url: 'https://www.elinashelest.com' } },
    { src: '/home-images/lp-hamburg-2024-encore.webp', attribution: { text: 'Jon Barker', url: 'https://www.instagram.com/jondbarker/', icon: 'pi-instagram' } },
    { src: '/home-images/lp-paris-2024-joe.webp', attribution: { text: 'Jon Barker', url: 'https://www.instagram.com/jondbarker/', icon: 'pi-instagram' } },
    { src: '/home-images/lp-paris-2024-em.webp', attribution: { text: 'Jon Barker', url: 'https://www.instagram.com/jondbarker/', icon: 'pi-instagram' } },
    { src: '/home-images/lp-hamburg-2024-colin.webp', attribution: { text: 'Jon Barker', url: 'https://www.instagram.com/jondbarker/', icon: 'pi-instagram' } },
    { src: '/home-images/lp-paris-2024-dave.webp', attribution: { text: 'Jon Barker', url: 'https://www.instagram.com/jondbarker/', icon: 'pi-instagram' } },
    { src: '/home-images/lp-ucl25.webp', attribution: { text: 'FelixSFD' } },
    { src: '/home-images/lp-hamburg-2024-mike.webp', attribution: { text: 'Jon Barker', url: 'https://www.instagram.com/jondbarker/', icon: 'pi-instagram' } },
    { src: '/home-images/lp-hamburg-2024-em.webp', attribution: { text: 'Jon Barker', url: 'https://www.instagram.com/jondbarker/', icon: 'pi-instagram' } },
    { src: '/home-images/lp-paris-2024-em-flying.webp', attribution: { text: 'Jon Barker', url: 'https://www.instagram.com/jondbarker/', icon: 'pi-instagram' } },
    { src: '/home-images/lp-hamburg-2024-em2.webp', attribution: { text: 'Jon Barker', url: 'https://www.instagram.com/jondbarker/', icon: 'pi-instagram' } },
  ];

  protected readonly loadedHeroImages = signal([this.heroImages[0]]);
  protected readonly currentHeroImage = signal(this.heroImages[0]);
  protected readonly previousHeroImage = signal<HeroImage | null>(null);
  protected readonly pendingHeroImage = signal<HeroImage | null>(null);
  protected readonly heroImageLoadFailed = signal(false);
  private nextHeroImageIndex = 1;
  private heroTimer: ReturnType<typeof setTimeout> | undefined;
  private heroImageRequestVersion = 0;
  private heroImageLoadingAutomatically = false;
  private heroSlideshowStarted = false;
  private heroReducedMotion: MediaQueryList | undefined;

  private readonly authService = inject(AuthService);
  private readonly oidcSecurityService = inject(OidcSecurityService);
  private readonly messageService = inject(MessageService);
  private readonly concertsService = inject(ConcertsService);
  private readonly subscriptionsService = inject(CalendarSubscriptionsService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly calendarReady = computed(() => this.authService.initialized() && !this.authService.initializationError());
  protected readonly calendarAuthError = this.authService.initializationError;
  protected readonly creatingCalendar = signal(false);
  protected readonly calendarManagementId = signal<string | null>(null);
  protected readonly calendarSettings = signal<CalendarSubscriptionSettingsDto>({
    includeDoorsTime: false,
    includeLpStageTime: true,
  });

  nextConcert: ConcertDetailsDto | null = null;
  nextAttendingConcert: ConcertDetailsDto | null = null;
  nextBookmarkedConcert: ConcertDetailsDto | null = null;
  onThisDayConcerts: ConcertDetailsDto[] = [];
  readonly today = new Date();

  isLoadingNextConcert: boolean = false;
  nextConcertLoadFailed: boolean = false;
  isLoadingAttendingConcert: boolean = false;
  isLoadingBookmarkedConcert: boolean = false;
  isLoadingOnThisDay: boolean = false;
  onThisDayLoadFailed: boolean = false;
  private userConcertRequestVersion = 0;

  newFeatureAlertDismissedToken: string | null = localStorage.getItem("alert.new-feature.dismissed-token");
  currentNewFeatureAlertToken: string = "739e7808-da76-48c8-9c21-a1e13915e7fd";

  isLoggedIn$: boolean = false;

  ngOnInit() {
    this.loadNextConcert().then();
    this.loadOnThisDay().then();

    this.authService.isAuthenticated$.subscribe(isAuthenticated => {
      console.debug("Home component is authenticated:", isAuthenticated);
      this.isLoggedIn$ = isAuthenticated;

      if (isAuthenticated) {
        console.debug("Load user's bookmarks etc.");
        const requestVersion = ++this.userConcertRequestVersion;
        this.loadNextBookmarkedConcert(requestVersion);
        this.loadNextAttendingConcert(requestVersion);
      } else {
        this.userConcertRequestVersion++;
        this.nextBookmarkedConcert = null;
        this.nextAttendingConcert = null;
        this.isLoadingBookmarkedConcert = false;
        this.isLoadingAttendingConcert = false;
      }
    });

  }

  /** Starts loading later photos only after the initial hero image has finished. */
  protected onHeroImageReady(image: HeroImage, failed = false): void {
    if (failed) console.warn(`Hero image could not be displayed: ${image.src}`);
    if (image === this.heroImages[0] && !this.heroSlideshowStarted) {
      this.heroSlideshowStarted = true;
      this.startHeroSlideshow();
    }
  }

  private startHeroSlideshow(): void {
    const reducedMotion = this.heroReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const onMotionChange = () => {
      clearTimeout(this.heroTimer);
      if (reducedMotion.matches && this.heroImageLoadingAutomatically) {
        this.heroImageRequestVersion++;
        this.heroImageLoadingAutomatically = false;
        this.pendingHeroImage.set(null);
      }
      if (!reducedMotion.matches) this.scheduleNextHeroImage();
    };
    reducedMotion.addEventListener('change', onMotionChange);
    this.destroyRef.onDestroy(() => {
      clearTimeout(this.heroTimer);
      reducedMotion.removeEventListener('change', onMotionChange);
    });
    this.scheduleNextHeroImage();
  }

  private scheduleNextHeroImage(): void {
    clearTimeout(this.heroTimer);
    if (this.heroSlideshowStarted && !this.heroReducedMotion?.matches &&
        !this.pendingHeroImage() && !this.destroyRef.destroyed && this.heroImages.length > 1) {
      this.heroTimer = setTimeout(() => this.showHeroImage(this.nextHeroImageIndex, true), 7000);
    }
  }

  /** Shows the selected photo and restarts the automatic slideshow. */
  protected selectHeroImage(index: number): void {
    this.showHeroImage(index, false);
  }

  private showHeroImage(index: number, automatic: boolean): void {
    clearTimeout(this.heroTimer);
    const nextImage = this.heroImages[index];
    const requestVersion = ++this.heroImageRequestVersion;
    this.heroImageLoadingAutomatically = automatic;
    this.heroImageLoadFailed.set(false);
    this.nextHeroImageIndex = (index + 1) % this.heroImages.length;

    if (nextImage === this.currentHeroImage()) {
      this.pendingHeroImage.set(null);
      this.heroImageLoadingAutomatically = false;
      this.scheduleNextHeroImage();
      return;
    }

    this.pendingHeroImage.set(nextImage);
    const image = new Image();
    image.fetchPriority = automatic ? 'low' : 'auto';
    image.src = nextImage.src;
    void image.decode().then(() => {
      if (this.destroyRef.destroyed || requestVersion !== this.heroImageRequestVersion) return;
      this.loadedHeroImages.update(images => images.includes(nextImage) ? images : [...images, nextImage]);
      this.previousHeroImage.set(this.currentHeroImage());
      this.currentHeroImage.set(nextImage);
    }).catch(error => {
      if (this.destroyRef.destroyed || requestVersion !== this.heroImageRequestVersion) return;
      console.warn(`Hero image could not be loaded: ${nextImage.src}`, error);
      if (!automatic) this.heroImageLoadFailed.set(true);
    }).finally(() => {
      if (this.destroyRef.destroyed || requestVersion !== this.heroImageRequestVersion) return;
      this.pendingHeroImage.set(null);
      this.heroImageLoadingAutomatically = false;
      this.scheduleNextHeroImage();
    });
  }


      /** Scrolls to the calendar feed card — temporary target for the hero CTA. */
  protected scrollToCalendarFeed(): void {
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    document.getElementById('calendar-feed')?.scrollIntoView({ behavior: reduceMotion ? 'auto' : 'smooth', block: 'start' });
  }


  onNewFeatureAlertClosed() {
    this.newFeatureAlertDismissedToken = this.currentNewFeatureAlertToken;
    localStorage.setItem("alert.new-feature.dismissed-token", this.currentNewFeatureAlertToken);
  }


  private async loadNextConcert() {
    this.isLoadingNextConcert = true;
    this.nextConcertLoadFailed = false;

    try {
      this.nextConcert = await this.concertsService.getUpcoming(1).then(c => c.at(0) ?? null);
    } catch (err) {
      console.warn("Next concert could not be loaded.", err);
      this.nextConcert = null;
      this.nextConcertLoadFailed = true;
    } finally {
      this.isLoadingNextConcert = false;
    }
  }

  private async loadOnThisDay() {
    this.isLoadingOnThisDay = true;
    this.onThisDayLoadFailed = false;

    try {
      const concerts = await this.concertsService.getConcertsOnThisDay(
        this.today.getMonth() + 1,
        this.today.getDate(),
      );
      this.onThisDayConcerts = concerts.sort((left, right) =>
        (right.postedStartTime ?? '').localeCompare(left.postedStartTime ?? ''));
    } catch (error) {
      console.error("Concerts for this day could not be loaded", error);
      this.onThisDayConcerts = [];
      this.onThisDayLoadFailed = true;
    } finally {
      this.isLoadingOnThisDay = false;
    }
  }

  protected retryOnThisDay(): void {
    void this.loadOnThisDay();
  }

  protected login(): void {
    this.oidcSecurityService.authorize();
  }

  /** Opens a matching subscription or creates one with the selected settings. */
  protected createCalendar(): void {
    if (!this.calendarReady() || this.creatingCalendar() || this.calendarManagementId()) return;

    this.creatingCalendar.set(true);
    const settings = this.calendarSettings();
    const subscriptions = this.oidcSecurityService.authenticated().isAuthenticated
      ? this.subscriptionsService.listMine().pipe(catchError(() => {
          this.messageService.add({
            severity: 'warn',
            summary: 'Could not check existing subscriptions',
            detail: 'A new subscription will be created and may duplicate an existing one.',
            life: 10000,
          });
          return of<CalendarSubscriptionDto[]>([]);
        }))
      : of<CalendarSubscriptionDto[]>([]);

    subscriptions.pipe(
      switchMap(items => {
        const match = items.find(item =>
          item.includeDoorsTime === settings.includeDoorsTime &&
          item.includeLpStageTime === settings.includeLpStageTime);
        return match ? of(match) : this.subscriptionsService.create(settings);
      }),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe({
      next: subscription => {
        this.calendarManagementId.set(subscription.id);
        void this.router.navigate(['/calendar-subscriptions', subscription.id]).then(navigated => {
          this.creatingCalendar.set(false);
          if (!navigated) {
            this.messageService.add({ severity: 'error', summary: 'Could not open subscription', detail: 'Use the management link on the card.' });
          }
        }).catch(() => {
          this.creatingCalendar.set(false);
          this.messageService.add({ severity: 'error', summary: 'Could not open subscription', detail: 'Use the management link on the card.' });
        });
      },
      error: error => {
        this.creatingCalendar.set(false);
        this.messageService.add({ severity: 'error', summary: 'Could not create subscription', detail: subscriptionErrorMessage(error) });
      },
    });
  }

  private loadNextBookmarkedConcert(requestVersion: number) {
    this.isLoadingBookmarkedConcert = true;
    void this.concertsService.getUpcomingBookmarkedConcerts().then(result => {
      if (requestVersion !== this.userConcertRequestVersion || !this.isLoggedIn$) return;
      this.nextBookmarkedConcert = result.at(0) ?? null;
    }).catch(err => {
      if (requestVersion !== this.userConcertRequestVersion || !this.isLoggedIn$) return;
      console.error("Next bookmarked concert could not be loaded", err);
      this.nextBookmarkedConcert = null;
    }).finally(() => {
      if (requestVersion === this.userConcertRequestVersion && this.isLoggedIn$) {
        this.isLoadingBookmarkedConcert = false;
      }
    });
  }


  private loadNextAttendingConcert(requestVersion: number) {
    this.isLoadingAttendingConcert = true;
    void this.concertsService.getUpcomingAttendingConcerts().then(result => {
      if (requestVersion !== this.userConcertRequestVersion || !this.isLoggedIn$) return;
      this.nextAttendingConcert = result.at(0) ?? null;
    }).catch(err => {
      if (requestVersion !== this.userConcertRequestVersion || !this.isLoggedIn$) return;
      console.error("Next concert you attend could not be loaded", err);
      this.nextAttendingConcert = null;
    }).finally(() => {
      if (requestVersion === this.userConcertRequestVersion && this.isLoggedIn$) {
        this.isLoadingAttendingConcert = false;
      }
    });
  }


}
