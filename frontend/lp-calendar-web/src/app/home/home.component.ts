import {Component, ElementRef, inject, Input, OnInit, ViewChild} from '@angular/core';
import {RouterLink} from '@angular/router';
import {environment} from '../../environments/environment';
import {ConcertCardComponent} from '../concert-card/concert-card.component';
import {ConcertsService} from '../services/concerts.service';
import {CalendarFeedBuilderComponent} from '../calendar-feed-builder/calendar-feed-builder.component';
import {ToastrService} from 'ngx-toastr';
import {MatomoTracker} from 'ngx-matomo-client';
import {ConcertDto} from '../modules/lpshows-api';
import {NgIf} from '@angular/common';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {NgbAlert} from '@ng-bootstrap/ng-bootstrap';
import {AuthService} from '../auth/auth.service';

@Component({
  selector: 'app-home',
  imports: [
    RouterLink,
    ConcertCardComponent,
    CalendarFeedBuilderComponent,
    NgIf,
    NgbAlert
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {

  protected readonly environment = environment;

  private readonly tracker = inject(MatomoTracker);
  private readonly authService = inject(AuthService);
  private readonly oidcSecurityService = inject(OidcSecurityService);

  nextConcert: ConcertDto | null = null;
  nextAttendingConcert: ConcertDto | null = null;
  nextBookmarkedConcert: ConcertDto | null = null;

  isLoadingNextConcert: boolean = false;
  isLoadingAttendingConcert: boolean = false;
  isLoadingBookmarkedConcert: boolean = false;

  newFeatureAlertDismissedToken: string | null = localStorage.getItem("alert.new-feature.dismissed-token");
  currentNewFeatureAlertToken: string = "c4b2d59a-a98d-4d46-8e73-854ded50125a";

  isLoggedIn$: boolean = false;

  iCalFeedUrl$: string = "";


  constructor(private concertsService: ConcertsService, private toastrService: ToastrService) {
  }


  ngOnInit() {
    this.loadNextConcert();

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


  onNewFeatureAlertClosed() {
    this.newFeatureAlertDismissedToken = this.currentNewFeatureAlertToken;
    localStorage.setItem("alert.new-feature.dismissed-token", this.currentNewFeatureAlertToken);
  }


  private loadNextConcert() {
    this.isLoadingNextConcert = true;
    this.concertsService.getNextConcert().subscribe({
      next: result => {
        this.nextConcert = result;
        this.isLoadingNextConcert = false;
      },
      error: err => {
        // If the request times out, an error will have been emitted.
        console.warn("Next concert was not found. Maybe there is nothing scheduled.");
        this.nextConcert = null;
        this.isLoadingNextConcert = false;
      }
    });
  }


  private loadNextBookmarkedConcert() {
    this.isLoadingBookmarkedConcert = true;
    this.concertsService.getNextBookmarked().subscribe({
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
    this.concertsService.getNextAttending().subscribe({
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
        this.toastrService.success("Copied URL to clipboard!")
      });
  }


  onFeedUrlUpdated(newUrl: string) {
    console.log("New URL: " + newUrl);
    this.iCalFeedUrl$ = newUrl;
  }
}
