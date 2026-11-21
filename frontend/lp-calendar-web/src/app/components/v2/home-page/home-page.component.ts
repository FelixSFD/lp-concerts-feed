import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {AuthService} from '../../../auth/auth.service';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {ConcertDto} from '../../../modules/lpshows-api';
import {LegacyConcertsService} from '../../../services/legacy-concerts.service';
import { environment } from "../../../../environments/environment";
import {Message} from 'primeng/message';
import {Card} from 'primeng/card';
import {Tag} from 'primeng/tag';
import {Button} from 'primeng/button';
import {MessageService} from 'primeng/api';
import {Router, RouterLink} from '@angular/router';
import {Divider} from 'primeng/divider';
import {FormsModule} from '@angular/forms';
import {CalendarFeedBuilderComponent} from '../calendar-feed-builder/calendar-feed-builder.component';
import {ConcertCardComponent} from '../concert-card/concert-card.component';
import { ConcertsService } from '../../../services/concerts.service';
import { CalendarSubscriptionDto, CalendarSubscriptionSettingsDto, ConcertDetailsDto } from '../../../modules/lpshows-api/v3';
import { CalendarSubscriptionsService } from '../../../services/calendar-subscriptions.service';
import { subscriptionErrorMessage } from '../calendar-subscriptions/calendar-subscription-errors';
import { catchError, of, switchMap } from 'rxjs';

@Component({
  selector: 'app-home-page',
  imports: [
    Message,
    Card,
    Tag,
    Button,
    RouterLink,
    Divider,
    FormsModule,
    CalendarFeedBuilderComponent,
    ConcertCardComponent
  ],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.css',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class HomePageComponent implements OnInit {
  protected readonly environment = environment;

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
  nextAttendingConcert: ConcertDto | null = null;
  nextBookmarkedConcert: ConcertDto | null = null;

  isLoadingNextConcert: boolean = false;
  isLoadingAttendingConcert: boolean = false;
  isLoadingBookmarkedConcert: boolean = false;

  newFeatureAlertDismissedToken: string | null = localStorage.getItem("alert.new-feature.dismissed-token");
  currentNewFeatureAlertToken: string = "739e7808-da76-48c8-9c21-a1e13915e7fd";

  isLoggedIn$: boolean = false;

  constructor(private legacyConcertsService: LegacyConcertsService) {
  }


  ngOnInit() {
    this.loadNextConcert().then();

    this.authService.isAuthenticated$.subscribe(isAuthenticated => {
      console.debug("Home component is authenticated:", isAuthenticated);
      this.isLoggedIn$ = isAuthenticated;

      if (isAuthenticated) {
        console.debug("Load user's bookmarks etc.");
        this.loadNextBookmarkedConcert();
        this.loadNextAttendingConcert();
      }
    });

  }


  login(): void {
    this.oidcSecurityService.authorize();
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

    try {
      this.nextConcert = await this.concertsService.getUpcoming(1).then(c => c.at(0) ?? null);
    } catch (err) {
      console.warn("Next concert was not found. Maybe there is nothing scheduled.", err);
      this.nextConcert = null;
    } finally {
      this.isLoadingNextConcert = false;
    }
  }


  private loadNextBookmarkedConcert() {
    this.isLoadingBookmarkedConcert = true;
    this.legacyConcertsService.getNextBookmarked().subscribe({
      next: result => {
        let next = result.at(0);
        if (next != undefined) {
          this.nextBookmarkedConcert = next;
        }

        this.isLoadingBookmarkedConcert = false;
      },
      error: err => {
        // If the request times out, an error will have been emitted.
        console.log(err);
        console.error("Next bookmarked concert could not be loaded");
        this.nextBookmarkedConcert = null;
        this.isLoadingBookmarkedConcert = false;
      }
    });
  }


  private loadNextAttendingConcert() {
    this.isLoadingAttendingConcert = true;
    this.legacyConcertsService.getNextAttending().subscribe({
      next: result => {
        let next = result.at(0);
        if (next != undefined) {
          this.nextAttendingConcert = next;
        }

        this.isLoadingAttendingConcert = false;
      },
      error: err => {
        // If the request times out, an error will have been emitted.
        console.log(err);
        console.error("Next concert you attend could not be loaded");
        this.nextAttendingConcert = null;
        this.isLoadingAttendingConcert = false;
      }
    });
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
}
