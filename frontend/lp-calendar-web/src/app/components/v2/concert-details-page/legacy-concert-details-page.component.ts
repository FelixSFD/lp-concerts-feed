import {ChangeDetectionStrategy, Component, inject, OnInit} from '@angular/core';
import {AuthService} from '../../../auth/auth.service';
import {MatomoTracker} from 'ngx-matomo-client';
import {
  AdjacentConcertsResponseDto,
  ConcertBookmarkUpdateRequestDto,
  ConcertDto,
  ConcertWithSetlistsDto,
  ErrorResponseDto,
  GetConcertBookmarkCountsResponseDto
} from '../../../modules/lpshows-api';
import {Setlist} from '../../../data/setlists/setlist';
import {ActivatedRoute, Router} from '@angular/router';
import {ConcertsService} from '../../../services/concerts.service';
import {Meta} from '@angular/platform-browser';
import {HttpErrorResponse} from '@angular/common/http';
import {DateTime} from 'luxon';
import {MenuItem, MessageService} from 'primeng/api';
import {ConcertDetailsComponent} from '../concert-details/concert-details.component';
import {ConcertDetailsViewModel} from '../concert-details/concert-details.view-model';

@Component({
  selector: 'app-legacy-concert-details-page',
  imports: [
    ConcertDetailsComponent
  ],
  templateUrl: './legacy-concert-details-page.component.html',
  styleUrl: './legacy-concert-details-page.component.css',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class LegacyConcertDetailsPageComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);
  private readonly concertsService = inject(ConcertsService);
  private readonly metaService = inject(Meta);
  private readonly route = inject(ActivatedRoute);

  tracker = inject(MatomoTracker);

  resolverError$: ErrorResponseDto | null = null;
  concert$: ConcertWithSetlistsDto | null = null;
  adjacentConcertData$: AdjacentConcertsResponseDto | null = null;
  concertBookmarks$: GetConcertBookmarkCountsResponseDto | null = null;
  concertBookmarksLoading$: boolean = false;
  setlists$: Setlist[] = [];
  setlistsCacheUpdatedAt$: DateTime | null = null;
  viewModel$: ConcertDetailsViewModel | null = null;

  isAuthenticated$ = false;
  canUpdateConcerts$ = false;
  canEditSetlists = false;

  addSetlistButtonItems: MenuItem[] = [];

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.addSetlistButtonItems = [
        {
          label: "Add new setlist",
          routerLink: ['/admin', 'setlists', 'add', params['id']]
        },
        {
          label: "Import setlist",
          routerLink: ['./import']
        }
      ];
    });

    this.route.data.subscribe(data => {
      console.debug("Resolved data:", data);

      let concert = data['concert'] as ConcertWithSetlistsDto | null;
      if (concert == null || !concert.id) {
        this.resolverError$ = data['concert'] as ErrorResponseDto;
        this.viewModel$ = null;
        return;
      }

      this.concert$ = concert;
      this.setlists$ = concert?.cachedSetlists?.map(s => Setlist.fromDto(s)) ?? [];
      this.setlistsCacheUpdatedAt$ = concert?.cachedSetlistsAt != null ? this.getDateTime(concert?.cachedSetlistsAt) : null;
      this.updateViewModel();

      this.loadAdjacentConcerts();
      this.loadBookmarkStatus();

      if (this.concert$ != null) {
        this.updateMetaInfo(this.concert$);
      }
    });

    this.authService.canUpdateConcerts.subscribe(hasPermission => {
      this.canUpdateConcerts$ = hasPermission;
    });
    this.authService.canManageSetlists.subscribe(hasPermission => {
      this.canEditSetlists = hasPermission;
    });
    this.authService.isAuthenticated$.subscribe(authenticated => {
      this.isAuthenticated$ = authenticated;
    });
  }

  onBookmarkClicked() {
    this.onBookmarkOrAttendingClicked(ConcertBookmarkUpdateRequestDto.StatusEnum.Bookmarked);
  }

  onAttendingClicked() {
    this.onBookmarkOrAttendingClicked(ConcertBookmarkUpdateRequestDto.StatusEnum.Attending);
  }

  onAddSetlistBtnClicked() {
    this.router.navigate(['/admin', 'setlists', 'add', this.concert$?.id]).then().catch((err) => {
      this.messageService.add({
        severity: "danger",
        summary: "Could not navigate to setlist",
        text: err.message,
      });
    });
  }

  private onBookmarkOrAttendingClicked(status: GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum) {
    console.log("Clicked button for: ", status);
    this.concertBookmarksLoading$ = true;
    this.updateViewModel();

    this.authService.isAuthenticated$.subscribe((isAuthenticated) => {
      if (this.concert$?.id == undefined || this.concertBookmarks$ == null) {
        this.messageService.add({
          severity: "error",
          summary: "Concert not loaded",
        });
        this.concertBookmarksLoading$ = false;
        this.updateViewModel();
        return;
      }

      if (isAuthenticated) {
        if (this.concertBookmarks$?.currentUserStatus == status) {
          // remove bookmark
          this.tracker.trackEvent("concert_bookmark", "remove", status);
          this.concertsService.setBookmarksForConcert(this.concert$?.id, ConcertBookmarkUpdateRequestDto.StatusEnum.None).subscribe({
            next: () => {
              this.concertBookmarks$!.currentUserStatus = GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum.None;
              this.loadBookmarkStatus();
            },
            error: (err: HttpErrorResponse) => {
              console.log(err);
              let errorResponse: ErrorResponseDto = err.error;
              this.messageService.add({
                severity: "error",
                summary: "Failed to remove bookmark!",
                text: errorResponse.message,
              });
              this.concertBookmarksLoading$ = false;
              this.updateViewModel();
            }
          });
        } else {
          // add bookmark
          this.tracker.trackEvent("concert_bookmark", "set", status);
          this.concertsService.setBookmarksForConcert(this.concert$?.id, status).subscribe({
            next: () => {
              this.concertBookmarks$!.currentUserStatus = status;
              this.loadBookmarkStatus();
            },
            error: (err: HttpErrorResponse) => {
              console.log(err);
              let errorResponse: ErrorResponseDto = err.error;
              this.messageService.add({
                severity: "danger",
                summary: "Failed to save bookmark!",
                text: errorResponse.message,
              });
              this.concertBookmarksLoading$ = false;
              this.updateViewModel();
            }
          });
        }
      } else {
        this.messageService.add({
          severity: "info",
          summary: "You are not logged in!",
        });
        this.concertBookmarksLoading$ = false;
        this.updateViewModel();
      }
    });
  }

  private loadBookmarkStatus() {
    let id = this.concert$?.id;
    if (id) {
      this.concertsService.getBookmarksForConcert(id)
        .subscribe(bookmarkStatus => {
          if (bookmarkStatus != undefined) {
            this.concertBookmarksLoading$ = false;
            this.concertBookmarks$ = bookmarkStatus;
            this.updateViewModel();
          }
        });
    }
  }

  private loadAdjacentConcerts() {
    let id = this.concert$?.id;
    if (id) {
      this.concertsService.getAdjacentConcerts(id)
        .subscribe(adjacentConcerts => {
          if (adjacentConcerts != undefined) {
            this.adjacentConcertData$ = adjacentConcerts;
            this.updateViewModel();
          }
        });
    }
  }

  private updateViewModel() {
    if (!this.concert$) {
      this.viewModel$ = null;
      return;
    }
    this.viewModel$ = ConcertDetailsViewModel.fromLegacyDto(
      this.concert$,
      this.adjacentConcertData$,
      this.concertBookmarks$,
      this.concertBookmarksLoading$,
      this.setlists$,
      this.setlistsCacheUpdatedAt$
    );
  }

  private updateMetaInfo(concert: ConcertDto) {
    let concertDateTitleExtension = "";
    if (concert.postedStartTime != undefined) {
      let concertDate = new Date(concert.postedStartTime);
      concertDateTitleExtension = " - " + concertDate.toLocaleDateString();
    }

    let titleInfo = concert.city + ", " + concert.country + concertDateTitleExtension;
    window.document.title = window.document.title.replace("Details", titleInfo);

    let pageTitle = "";
    let description = "";

    let concertDateDescriptionExtension = "";
    if (concert.postedStartTime != undefined) {
      let concertDate = new Date(concert.postedStartTime);
      concertDateDescriptionExtension = concertDate.toLocaleDateString() + ": ";
    }

    if (concert.tourName != undefined) {
      pageTitle = concert.tourName + ": " + concert.city;
      description = concertDateDescriptionExtension + "Linkin Park show of the " + concert.tourName + " in " + concert.city + ", " + concert.country;
    } else {
      pageTitle = "Linkin Park at " + concert.venue;
      description = concertDateDescriptionExtension + "Linkin Park show at " + concert.venue + " in " + concert.city + ", " + concert.country;
    }

    this.metaService.updateTag({
      name: "title",
      content: pageTitle
    });
    this.metaService.updateTag({
      property: "og:title",
      content: pageTitle
    });
    this.metaService.updateTag({
      name: "description",
      content: description
    });
    this.metaService.updateTag({
      property: "og:description",
      content: description
    });
  }

  public getDateTime(inputDate: string) {
    return DateTime.fromISO(inputDate, {setZone: false});
  }
}
