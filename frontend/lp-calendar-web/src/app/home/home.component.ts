import {Component, inject, Input, OnInit} from '@angular/core';
import {RouterLink} from '@angular/router';
import {environment} from '../../environments/environment';
import {ConcertCardComponent} from '../concert-card/concert-card.component';
import {ConcertsService} from '../services/concerts.service';
import {Concert} from '../data/concert';
import {CalendarFeedBuilderComponent} from '../calendar-feed-builder/calendar-feed-builder.component';
import {ToastrService} from 'ngx-toastr';
import {MatomoTracker} from 'ngx-matomo-client';

@Component({
  selector: 'app-home',
  imports: [
    RouterLink,
    ConcertCardComponent,
    CalendarFeedBuilderComponent
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {

  protected readonly environment = environment;

  private readonly tracker = inject(MatomoTracker);

  nextConcert: Concert | null = null;

  iCalFeedUrl$: string = "";


  constructor(private concertsService: ConcertsService, private toastrService: ToastrService) {
  }


  ngOnInit() {
    this.concertsService.getNextConcert().subscribe(result => this.nextConcert = result);
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
    let calendarUrl = environment.apiCachedBaseUrl + "/Prod/feed/ical";
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
