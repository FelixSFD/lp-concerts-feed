import {Component, ElementRef, inject, OnInit, ViewChild} from '@angular/core';
import {AuthService} from '../../../auth/auth.service';
import {ToastrService} from 'ngx-toastr';
import {MatomoTracker} from 'ngx-matomo-client';
import {
  AdjacentConcertsResponseDto,
  ConcertBookmarkUpdateRequestDto,
  ConcertDto, ConcertWithSetlistsDto,
  ErrorResponseDto,
  GetConcertBookmarkCountsResponseDto
} from '../../../modules/lpshows-api';
import {load, MapKit} from '@apple/mapkit-loader';
import {Map as AppleMap} from 'apple-mapkit/mapkit';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {Setlist} from '../../../data/setlists/setlist';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import {ConcertsService} from '../../../services/concerts.service';
import {SetlistsService} from '../../../services/setlists.service';
import {Meta} from '@angular/platform-browser';
import {HttpErrorResponse} from '@angular/common/http';
import {DateTime} from 'luxon';
import {environment} from '../../../../environments/environment';
import {ConcertTitleGenerator} from '../../../data/concert-title-generator';
import {ConcertBadgesComponent} from '../concert-badges/concert-badges.component';
import {ProgressSpinner} from 'primeng/progressspinner';
import {Card} from 'primeng/card';
import {SplitButton} from 'primeng/splitbutton';
import {NgTemplateOutlet} from '@angular/common';
import {Button} from 'primeng/button';
import {ButtonGroup} from 'primeng/buttongroup';
import {MenuItem, MessageService} from 'primeng/api';
import {FormsModule} from '@angular/forms';
import {Tooltip} from 'primeng/tooltip';
import {TimeSpanPipe} from '../../../data/time-span-pipe';
import {CountdownComponent} from '../countdown/countdown.component';
import {Message} from 'primeng/message';
import {NgbTooltip} from '@ng-bootstrap/ng-bootstrap';
import {SetlistComponent} from '../setlists/setlist/setlist.component';
import {Tag} from 'primeng/tag';

@Component({
  selector: 'app-concert-details-page',
  imports: [
    ConcertBadgesComponent,
    ProgressSpinner,
    Card,
    SplitButton,
    NgTemplateOutlet,
    Button,
    RouterLink,
    ButtonGroup,
    FormsModule,
    Tooltip,
    TimeSpanPipe,
    CountdownComponent,
    CountdownComponent,
    Message,
    NgbTooltip,
    SetlistComponent,
    Tag
  ],
  templateUrl: './concert-details-page.component.html',
  styleUrl: './concert-details-page.component.css',
})
export class ConcertDetailsPageComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly toastr = inject(ToastrService);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);
  tracker = inject(MatomoTracker);

  resolverError$: ErrorResponseDto | null = null;
  concert$: ConcertDto | null = null;
  adjacentConcertData$: AdjacentConcertsResponseDto | null = null;
  concertBookmarks$: GetConcertBookmarkCountsResponseDto | null = null;
  concertBookmarksLoading$: boolean = false;
  concertId: string | undefined;

  // Apple Maps
  private mapKit: MapKit | undefined;
  private appleMap: AppleMap | undefined;

  // Service to check auth information
  private readonly oidcSecurityService = inject(OidcSecurityService);

  isAuthenticated$ = false;
  canUpdateConcerts$ = false;
  canEditSetlists = false;

  setlists$: Setlist[] = [];
  setlistsCacheUpdatedAt$: DateTime | null = null;

  addSetlistButtonItems: MenuItem[] = [];

  constructor(private route: ActivatedRoute, private concertsService: ConcertsService, private setlistService: SetlistsService, private metaService: Meta) {
  }


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

      if (data['concert'].type === 'ErrorResponseDto') {
        this.resolverError$ = data['concert'];

        return;
      }

      let concert = data['concert'] as ConcertWithSetlistsDto;
      this.loadAdjacentConcerts();
      this.loadBookmarkStatus();

      if (this.concert$ != null) {
        this.updateMetaInfo(this.concert$);
      }

      this.concert$ = concert;
      this.setlists$ = concert?.cachedSetlists?.map(s => Setlist.fromDto(s)) ?? [];
      this.setlistsCacheUpdatedAt$ = concert?.cachedSetlistsAt != null ? this.getDateTime(concert?.cachedSetlistsAt) : null;
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
    this.router.navigate(['/admin', 'setlists', 'add', this.concertId]).then().catch((err) => {
      this.toastr.error(err)
    });
  }


  private async initAppleMaps() {
    this.mapKit = await load({
      token: environment.appleMapsToken,
      language: "en-US",
      libraries: ["map", "annotations"],
    });
  }


  @ViewChild('appleMaps')
  set appleMaps(mapElement: ElementRef<HTMLDivElement> | undefined) {
    if (!mapElement) return;
    if (!this.appleMaps) {
      console.debug('MapKit not initialized yet!');
      this.initAppleMaps().then(() => {
        this.appleMap = this.makeMap(mapElement.nativeElement);
        this.fillMapData();
      });
      return;
    }

    console.log("Will set map element: ", mapElement);
    this.appleMap = this.makeMap(mapElement.nativeElement);
    this.fillMapData();
  }


  private makeMap(mapElement: HTMLDivElement) {
    let map = new this.mapKit!.Map(mapElement);
    map.colorScheme = "adaptive";
    return map;
  }


  private fillMapData() {
    this.addOrMoveMarker(this.concert$?.venueLongitude ?? 0, this.concert$?.venueLatitude ?? 0);
  }


  private onBookmarkOrAttendingClicked(status: GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum) {
    console.log("Clicked button for: ", status);
    this.concertBookmarksLoading$ = true;

    this.authService.isAuthenticated$.subscribe((isAuthenticated) => {
      if (this.concertId == undefined || this.concertBookmarks$ == null) {
        this.toastr.error("Concert not loaded");
        this.concertBookmarksLoading$ = false;
        return;
      }

      if (isAuthenticated) {
        if (this.concertBookmarks$?.currentUserStatus == status) {
          // remove bookmark
          this.tracker.trackEvent("concert_bookmark", "remove", status);
          this.concertsService.setBookmarksForConcert(this.concertId, ConcertBookmarkUpdateRequestDto.StatusEnum.None).subscribe({
            next: () => {
              //this.toastr.success("Removed bookmark!");
              this.concertBookmarks$!.currentUserStatus = GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum.None;
              this.loadBookmarkStatus();
            },
            error: (err: HttpErrorResponse) => {
              console.log(err);
              let errorResponse: ErrorResponseDto = err.error;
              this.toastr.error(errorResponse.message, "Failed to remove bookmark!");
            }
          });
        } else {
          // add bookmark
          this.tracker.trackEvent("concert_bookmark", "set", status);
          this.concertsService.setBookmarksForConcert(this.concertId, status).subscribe({
            next: () => {
              //this.toastr.success("Added bookmark!");
              this.concertBookmarks$!.currentUserStatus = status;
              this.loadBookmarkStatus(); // will set the loading status to false when done
            },
            error: (err: HttpErrorResponse) => {
              console.log(err);
              let errorResponse: ErrorResponseDto = err.error;
              this.toastr.error(errorResponse.message, "Failed to save bookmark!");
              this.concertBookmarksLoading$ = false;
            }
          });
        }
      } else {
        this.toastr.info('You are not logged in!');
        this.concertBookmarksLoading$ = false;
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
          }
        });
    }
  }


  loadDataForId(id: string | undefined) {
    console.log("loadDataForId: " + id);
    if (id == undefined) {
      this.concertId = undefined;
      this.concert$ = null;
      this.adjacentConcertData$ = null;
      return;
    }

    this.concertId = id!;

    // delete current concert data to show a loading spinner
    this.concert$ = null;
    this.setlists$ = [];

    this.loadAdjacentConcerts();

    this.loadBookmarkStatus();

    this.concertsService
      .getConcert(this.concertId)
      .subscribe(result => {
        if (result != undefined) {
          let concertDateTitleExtension = "";
          if (result.postedStartTime != undefined) {
            let concertDate = new Date(result.postedStartTime);
            concertDateTitleExtension = " - " + concertDate.toLocaleDateString();
          }

          let titleInfo = result.city + ", " + result.country + concertDateTitleExtension;
          window.document.title = window.document.title.replace("Details", titleInfo);

          this.updateMetaInfo(result);
        }

        // Set coordinates in map
        console.log(result);
        if (result.venueLongitude != undefined && result.venueLatitude != undefined
          && result.venueLongitude != 0 && result.venueLatitude != 0) {
          this.addOrMoveMarker(result.venueLongitude, result.venueLatitude);
        }

        this.setlists$ = result.cachedSetlists?.map(s => Setlist.fromDto(s)) ?? [];
        this.setlistsCacheUpdatedAt$ = result.cachedSetlistsAt != null ? this.getDateTime(result.cachedSetlistsAt) : null;

        return this.concert$ = result;
      });
  }


  private zoomToCoordinates(lon: number, lat: number) {
    if (this.appleMap && this.mapKit) {
      this.appleMap.region = new this.mapKit.CoordinateRegion(
        new this.mapKit.Coordinate(lat, lon),
        new this.mapKit.CoordinateSpan(0.06, 0.2)
      );
    }
  }


  private getVenuePinTitle() {
    let venue = this.concert$?.venue ?? undefined;
    let city = this.concert$?.city ?? undefined;

    if (venue == undefined) {
      return city ?? undefined;
    } else if (city != undefined) {
      return venue + ", " + city;
    } else {
      return undefined;
    }
  }


  private addOrMoveMarker(lon: number, lat: number) {
    if (!this.appleMap || !this.mapKit) {
      return;
    }
    const annotation = new this.mapKit!.MarkerAnnotation(new this.mapKit!.Coordinate(lat, lon), {
      color: "#c969e0",
      map: this.appleMap,
      title: this.getVenuePinTitle()
    });
    this.appleMap?.showItems([annotation]);

    this.zoomToCoordinates(lon, lat);
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
      description = concertDateDescriptionExtension + "Linkin Park show of the " + concert.tourName + " in " + concert.city + ", " + concert.country
    } else {
      pageTitle = "Linkin Park at " + concert.venue;
      description = concertDateDescriptionExtension + "Linkin Park show at " + concert.venue + " in " + concert.city + ", " + concert.country
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


  openLinkinpediaClicked() {
    if (this.concert$ == undefined) {
      return;
    }

    let dt = this.getDateTimeInTimezone(this.concert$!.postedStartTime!, this.concert$.timeZoneId!);
    let wikiLink = "https://linkinpedia.com/wiki/Live:" + dt.toFormat("yyyyMMdd");

    this.tracker.trackLink(wikiLink, "link");
    window.open(wikiLink, "_blank");
  }


  public getDateTime(inputDate: string) {
    return DateTime.fromISO(inputDate, {setZone: false});
  }


  public getDateTimeInTimezone(inputDate: string, timeZoneId: string) {
    return DateTime.fromISO(inputDate, {zone: timeZoneId});
  }

  protected readonly ConcertTitleGenerator = ConcertTitleGenerator;
  protected readonly DateTime = DateTime;
  protected readonly String = String;
  protected readonly GetConcertBookmarkCountsResponseDto = GetConcertBookmarkCountsResponseDto;
  protected readonly environment = environment;
}
