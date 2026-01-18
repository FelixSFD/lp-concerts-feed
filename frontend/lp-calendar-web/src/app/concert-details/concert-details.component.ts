import {AfterViewInit, Component, ContentChild, ElementRef, inject, OnInit, ViewChild} from '@angular/core';
import {ConcertsService} from '../services/concerts.service';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {DateTime} from 'luxon';

import {CountdownComponent} from '../countdown/countdown.component';
import {Meta} from '@angular/platform-browser';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {ConcertBadgesComponent} from '../concert-badges/concert-badges.component';
import {environment} from '../../environments/environment';
import {ConcertTitleGenerator} from '../data/concert-title-generator';
import {TimeSpanPipe} from '../data/time-span-pipe';
import {NgbTooltip} from '@ng-bootstrap/ng-bootstrap';
import {MatomoTracker} from 'ngx-matomo-client';
import {
  AdjacentConcertsResponseDto,
  ConcertBookmarkUpdateRequestDto,
  ConcertDto,
  ErrorResponseDto,
  GetConcertBookmarkCountsResponseDto
} from '../modules/lpshows-api';
import {AuthService} from '../auth/auth.service';
import {ToastrService} from 'ngx-toastr';
import {HttpErrorResponse} from '@angular/common/http';
import {load, MapKit, Map as AppleMap} from '@apple/mapkit-loader';

@Component({
  selector: 'app-concert-details',
  imports: [
    CountdownComponent,
    RouterLink,
    ConcertBadgesComponent,
    TimeSpanPipe,
    NgbTooltip
],
  templateUrl: './concert-details.component.html',
  styleUrl: './concert-details.component.css'
})
export class ConcertDetailsComponent implements OnInit, AfterViewInit {
  private readonly authService = inject(AuthService);
  private readonly toastr = inject(ToastrService);
  tracker = inject(MatomoTracker);

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

  hasWriteAccess$ = false;

  constructor(private route: ActivatedRoute, private concertsService: ConcertsService, private metaService: Meta) {
    this.oidcSecurityService.checkAuth().subscribe(({ isAuthenticated }) => {
      this.hasWriteAccess$ = isAuthenticated;
    });
  }


  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.loadDataForId(params['id']);
    });
  }


  ngAfterViewInit() {
  }


  onBookmarkClicked() {
    this.onBookmarkOrAttendingClicked(ConcertBookmarkUpdateRequestDto.StatusEnum.Bookmarked);
  }


  onAttendingClicked() {
    this.onBookmarkOrAttendingClicked(ConcertBookmarkUpdateRequestDto.StatusEnum.Attending);
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
    if (this.concertId == undefined) {
      return;
    }

    this.concertsService.getBookmarksForConcert(this.concertId)
      .subscribe(bookmarkStatus => {
        if (bookmarkStatus != undefined) {
          this.concertBookmarksLoading$ = false;
          this.concertBookmarks$ = bookmarkStatus;
        }
      });
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

    this.concertsService.getAdjacentConcerts(this.concertId)
      .subscribe(adjacentConcerts => {
        if (adjacentConcerts != undefined) {
          this.adjacentConcertData$ = adjacentConcerts;
        }
      });

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


  protected readonly DateTime = DateTime;
  protected readonly environment = environment;
  protected readonly ConcertTitleGenerator = ConcertTitleGenerator;
  protected readonly window = window;
  protected readonly GetConcertBookmarkCountsResponseDto = GetConcertBookmarkCountsResponseDto;
}
