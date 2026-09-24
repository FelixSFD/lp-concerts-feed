import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import {MatomoTracker} from 'ngx-matomo-client';
import {AuthService} from '../../../auth/auth.service';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {ConcertDto} from '../../../modules/lpshows-api';
import {LegacyConcertsService} from '../../../services/legacy-concerts.service';
import { environment } from "../../../../environments/environment";
import {Message} from 'primeng/message';
import {Card} from 'primeng/card';
import {Button} from 'primeng/button';
import {SplitButton} from 'primeng/splitbutton';
import {MenuItem, MessageService} from 'primeng/api';
import {RouterLink} from '@angular/router';
import {Divider} from 'primeng/divider';
import {FormsModule} from '@angular/forms';
import {CalendarFeedBuilderComponent} from '../calendar-feed-builder/calendar-feed-builder.component';
import {ConcertCardComponent} from '../concert-card/concert-card.component';
import { ConcertsService } from '../../../services/concerts.service';
import { ConcertDetailsDto } from '../../../modules/lpshows-api/v3';

@Component({
  selector: 'app-home-page',
  imports: [
    Message,
    Card,
    Button,
    SplitButton,
    RouterLink,
    Divider,
    FormsModule,
    CalendarFeedBuilderComponent,
    ConcertCardComponent,
    ConcertCardComponent
  ],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.css',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class HomePageComponent implements OnInit {
  protected readonly environment = environment;

  private readonly tracker = inject(MatomoTracker);
  private readonly authService = inject(AuthService);
  private readonly oidcSecurityService = inject(OidcSecurityService);
  private readonly messageService = inject(MessageService);
  private readonly concertsService = inject(ConcertsService);

  nextConcert: ConcertDetailsDto | null = null;
  nextAttendingConcert: ConcertDto | null = null;
  nextBookmarkedConcert: ConcertDto | null = null;

  isLoadingNextConcert: boolean = false;
  isLoadingAttendingConcert: boolean = false;
  isLoadingBookmarkedConcert: boolean = false;

  newFeatureAlertDismissedToken: string | null = localStorage.getItem("alert.new-feature.dismissed-token");
  currentNewFeatureAlertToken: string = "739e7808-da76-48c8-9c21-a1e13915e7fd";

  isLoggedIn$: boolean = false;

  iCalFeedUrl$: string = "";

  iCalButtonItems$: MenuItem[] = [];


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

    this.iCalButtonItems$ = [
      {
        label: 'Subscribe',
        icon: 'pi pi-calendar-plus',
        command: (evt => {
          this.subscribeBtnClicked()
        }),
      },
      {
        label: 'Copy link',
        icon: 'pi pi-copy',
        command: (evt => {
          this.copyFeedUrlBtnClicked()
        }),
      }
    ];
  }


  login(): void {
    this.oidcSecurityService.authorize();
  }


  onNewFeatureAlertClosed() {
    this.newFeatureAlertDismissedToken = this.currentNewFeatureAlertToken;
    localStorage.setItem("alert.new-feature.dismissed-token", this.currentNewFeatureAlertToken);
  }


  private async loadNextConcert() {
    this.isLoadingNextConcert = true;

    try {
      this.nextConcert = await this.concertsService.getUpcoming(1).then();
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


  private getCalFeedUrl() {
    let calendarUrl = this.iCalFeedUrl$;
    return calendarUrl.replace("https", "webcal");
  }


  subscribeCustomBtnClicked() {
    let calendarUrl = this.getCalFeedUrl();
    this.tracker.trackEvent("ical_sub", "subscribed direct", calendarUrl);
    window.open(calendarUrl);
  }


  subscribeBtnClicked() {
    let calendarUrl = environment.apiBaseUrlLatest + "/feed/ical";
    calendarUrl = calendarUrl.replace("https", "webcal");
    window.open(calendarUrl);
  }


  copyFeedUrlBtnClicked() {
    let calendarUrl = this.getCalFeedUrl();
    this.tracker.trackEvent("ical_sub", "copied link", calendarUrl);
    navigator.clipboard.writeText(calendarUrl)
      .then(_ => {
        console.debug("copied iCal URL: " + calendarUrl);
        this.messageService.add({
          severity: "success",
          summary: "Copied URL to clipboard!",
        });
      });
  }


  onFeedUrlUpdated(newUrl: string) {
    console.log("New URL: " + newUrl);
    this.iCalFeedUrl$ = newUrl;
  }
}
