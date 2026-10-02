import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { ConcertDetailsComponent } from '../concert-details/concert-details.component';
import { ConcertDetailsViewModel } from '../concert-details/concert-details.view-model';
import {
  AdjacentConcertsResponseDto,
  ErrorResponseDto
} from '../../../modules/lpshows-api';
import { MenuItem, MessageService } from 'primeng/api';
import { ActivatedRoute, Router } from '@angular/router';
import {
  ConcertBookmarkUpdateRequestDto,
  ConcertDetailsDto,
  GetConcertBookmarkCountsResponseDto
} from '../../../modules/lpshows-api/v3';
import { AuthService } from '../../../auth/auth.service';
import { Meta } from '@angular/platform-browser';
import { ToursService } from '../../../services/tours.service';
import { ConcertsService } from '../../../services/concerts.service';

@Component({
  selector: 'app-concert-details-page',
  imports: [
    ConcertDetailsComponent
  ],
  templateUrl: './concert-details-page.component.html',
  styleUrl: './concert-details-page.component.css',
})
export class ConcertDetailsPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly metaService = inject(Meta);
  private readonly messageService = inject(MessageService);
  private readonly toursService = inject(ToursService);
  private readonly concertsService = inject(ConcertsService);

  detailsViewModel = signal<ConcertDetailsViewModel | null>(null);
  resolverError = signal<ErrorResponseDto | null>(null);

  isAuthenticated = signal<boolean>(false);
  canUpdateConcerts = signal<boolean>(false);
  canEditSetlists = signal<boolean>(false);
  concertBookmarks = signal<GetConcertBookmarkCountsResponseDto | null>(null);
  concertBookmarksLoading = signal<boolean>(false);

  private updateViewModelEffect = effect(() => {
    if (!this.concert) {
      this.detailsViewModel.set(null);
      return;
    }

    this.detailsViewModel.set(ConcertDetailsViewModel.fromV3Dto(
      this.concert,
      this.adjacentConcertData,
      this.concertBookmarks(),
      this.concertBookmarksLoading(),
      []
    ));
  });

  adjacentConcertData: AdjacentConcertsResponseDto | null = null;

  concert: ConcertDetailsDto | null = null;

  addSetlistButtonItems = signal<MenuItem[]>([]);

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.addSetlistButtonItems.set([
        {
          label: "Add new setlist",
          routerLink: ['/admin', 'setlists', 'add', params['id']]
        },
        {
          label: "Import setlist",
          routerLink: ['./import']
        }
      ]);
    });

    this.route.data.subscribe(data => {
      console.debug("Resolved data:", data);

      let concert = data['concert'] as ConcertDetailsDto | null;
      if (concert == null || !concert.id) {
        this.resolverError.set(data['concert'] as ErrorResponseDto);
        this.detailsViewModel.set(null);
        return;
      }

      this.concert = concert;
      //this.setlists$ = concert?.cachedSetlists?.map(s => Setlist.fromDto(s)) ?? [];
      //this.setlistsCacheUpdatedAt$ = concert?.cachedSetlistsAt != null ? this.getDateTime(concert?.cachedSetlistsAt) : null;
      this.updateViewModel();

      this.loadAdjacentConcerts()
        .then(() => this.updateViewModel());
      this.loadBookmarkStatus()
        .then(() => this.updateViewModel());

      if (this.concert != null) {
        this.updateMetaInfo(this.concert);
      }
    });

    this.authService.canUpdateConcerts.subscribe(hasPermission => {
      this.canUpdateConcerts.set(hasPermission);
    });
    this.authService.canManageSetlists.subscribe(hasPermission => {
      this.canEditSetlists.set(hasPermission);
    });
    this.authService.isAuthenticated$.subscribe(authenticated => {
      this.isAuthenticated.set(authenticated);
    });
  }

  private updateViewModel() {
    if (!this.concert) {
      this.detailsViewModel.set(null);
      return;
    }

    this.detailsViewModel.set(ConcertDetailsViewModel.fromV3Dto(
      this.concert,
      this.adjacentConcertData,
      this.concertBookmarks(),
      this.concertBookmarksLoading(),
      []
    ));
  }

  private updateMetaInfo(concert: ConcertDetailsDto) {
    let concertDateTitleExtension = "";
    if (concert.postedStartTime != undefined) {
      let concertDate = new Date(concert.postedStartTime);
      concertDateTitleExtension = " - " + concertDate.toLocaleDateString();
    }

    let venueDto = concert.venue;

    let titleInfo = venueDto.city.name + ", " + venueDto.city.country.name + concertDateTitleExtension;
    window.document.title = window.document.title.replace("Details", titleInfo);

    let pageTitle = "";
    let description = "";

    let concertDateDescriptionExtension = "";
    if (concert.postedStartTime != undefined) {
      let concertDate = new Date(concert.postedStartTime);
      concertDateDescriptionExtension = concertDate.toLocaleDateString() + ": ";
    }

    if (concert.tour?.name) {
      pageTitle = concert.tour.name + ": " + venueDto.city.name;
      description = concertDateDescriptionExtension + "Linkin Park show of the " + concert.tour.name + " in " + venueDto.city.name + ", " + venueDto.city.country.name;
    } else {
      pageTitle = "Linkin Park at " + venueDto.currentName;
      description = concertDateDescriptionExtension + "Linkin Park show at " + venueDto.currentName + " in " + venueDto.city.name + ", " + venueDto.city.country.name;
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

  onBookmarkClicked() {
    this.onBookmarkOrAttendingClicked(ConcertBookmarkUpdateRequestDto.StatusEnum.Bookmarked);
  }

  onAttendingClicked() {
    this.onBookmarkOrAttendingClicked(ConcertBookmarkUpdateRequestDto.StatusEnum.Attending);
  }

  private onBookmarkOrAttendingClicked(status: GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum) {
    console.debug("onBookmarkOrAttendingClicked", status);
    this.concertBookmarks.update(old => {
      let isRemoving = old?.currentUserStatus === status;
      console.debug("isRemoving", isRemoving);
      let addCount = isRemoving ? -1 : 1;
      let newUserStatus = isRemoving ? GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum.None : status;
      console.debug("newUserStatus", newUserStatus);

      this.concertsService.setBookmarkStatusForConcert(this.concert!.id, newUserStatus)
        .then(() => this.loadBookmarkStatus())
        .catch((err) => {
          console.error("Failed to set bookmark status", err);
          this.messageService.add({severity: "error", summary: "Failed to set bookmark status", detail: err.message})
        });

      if (status == GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum.Bookmarked) {
        return {
          ...old,
          currentUserStatus: newUserStatus,
          bookmarked: (old?.bookmarked ?? 0) + addCount
        }
      } else if (status == GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum.Attending) {
        return {
          ...old,
          currentUserStatus: newUserStatus,
          attending: (old?.attending ?? 0) + addCount
        }
      } else {
        console.warn("Unknown status clicked", status);
        return old;
      }
    });
  }

  onAddSetlistBtnClicked() {
    this.router.navigate(['/admin', 'setlists', 'add', this.concert?.id]).then().catch((err) => {
      this.messageService.add({
        severity: "danger",
        summary: "Could not navigate to setlist",
        text: err.message,
      });
    });
  }

  private async loadAdjacentConcerts() {
    if (!this.concert) {
      return;
    }

    console.debug("Loading adjacent concerts...");
    this.adjacentConcertData = await this.toursService.getAdjacentConcerts(this.concert!.id);
  }

  private async loadBookmarkStatus() {
    this.concertBookmarksLoading.set(true);
    this.concertBookmarks.set(await this.concertsService.getBookmarkStatusForConcert(this.concert!.id));
    this.concertBookmarksLoading.set(false);
  }
}
