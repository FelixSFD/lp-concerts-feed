import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import {AuthService} from '../../../auth/auth.service';
import { environment } from "../../../../environments/environment";
import {Message} from 'primeng/message';
import {Card} from 'primeng/card';
import {Tag} from 'primeng/tag';
import {Button} from 'primeng/button';
import {RouterLink} from '@angular/router';
import {ConcertCardComponent} from '../concert-card/concert-card.component';
import { ConcertsService } from '../../../services/concerts.service';
import { ConcertDetailsDto } from '../../../modules/lpshows-api/v3';
import { HeroCountdownComponent } from '../hero-countdown/hero-countdown.component';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-home-page',
  imports: [
    Message,
    Card,
    Tag,
    Button,
    RouterLink,
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

  private readonly authService = inject(AuthService);
  private readonly concertsService = inject(ConcertsService);

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
