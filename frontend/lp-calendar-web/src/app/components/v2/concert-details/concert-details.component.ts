import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  EventEmitter,
  inject,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  ViewChild
} from '@angular/core';
import {MatomoTracker} from 'ngx-matomo-client';
import {
  AdjacentConcertsResponseDto,
  ConcertDto,
  ErrorResponseDto,
  GetConcertBookmarkCountsResponseDto
} from '../../../modules/lpshows-api';
import {load, MapKit} from '@apple/mapkit-loader';
import {Map as AppleMap} from 'apple-mapkit/mapkit';
import {Setlist} from '../../../data/setlists/setlist';
import {RouterLink} from '@angular/router';
import {DiscordShareService} from '../../../services/discord-share.service';
import {DateTime} from 'luxon';
import {environment} from '../../../../environments/environment';
import {ConcertTitleGenerator} from '../../../data/concert-title-generator';
import {ConcertBadgesComponent} from '../concert-badges/concert-badges.component';
import {Card} from 'primeng/card';
import {SplitButton} from 'primeng/splitbutton';
import {Button} from 'primeng/button';
import {MenuItem, MessageService} from 'primeng/api';
import {FormsModule} from '@angular/forms';
import {Tooltip} from 'primeng/tooltip';
import {TimeSpanPipe} from '../../../data/time-span-pipe';
import {HeroCountdownComponent} from '../hero-countdown/hero-countdown.component';
import {SetlistComponent} from '../setlists/setlist/setlist.component';
import {Tag} from 'primeng/tag';
import {Image} from 'primeng/image';

@Component({
  selector: 'app-concert-details',
  imports: [
    ConcertBadgesComponent,
    Card,
    SplitButton,
    Button,
    RouterLink,
    FormsModule,
    Tooltip,
    TimeSpanPipe,
    HeroCountdownComponent,
    SetlistComponent,
    Tag,
    Image
  ],
  templateUrl: './concert-details.component.html',
  styleUrl: './concert-details.component.css',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class ConcertDetailsComponent implements OnChanges {
  private readonly messageService = inject(MessageService);
  protected readonly tracker = inject(MatomoTracker);
  protected readonly discordShare = inject(DiscordShareService);

  @Input() concert: ConcertDto | null = null;
  @Input() adjacentConcertData: AdjacentConcertsResponseDto | null = null;
  @Input() concertBookmarks: GetConcertBookmarkCountsResponseDto | null = null;
  @Input() concertBookmarksLoading: boolean = false;
  @Input() resolverError: ErrorResponseDto | null = null;
  @Input() isAuthenticated: boolean = false;
  @Input() canUpdateConcerts: boolean = false;
  @Input() canEditSetlists: boolean = false;
  @Input() setlists: Setlist[] = [];
  @Input() setlistsCacheUpdatedAt: DateTime | null = null;
  @Input() addSetlistButtonItems: MenuItem[] = [];

  @Output() bookmarkClicked = new EventEmitter<void>();
  @Output() attendingClicked = new EventEmitter<void>();
  @Output() addSetlistClicked = new EventEmitter<void>();

  // Apple Maps
  private mapKit: MapKit | undefined;
  private appleMap: AppleMap | undefined;
  private mapElementRef: ElementRef<HTMLDivElement> | undefined;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['concert'] && this.appleMap && this.mapKit) {
      this.fillMapData();
    }
  }

  onBookmarkClicked() {
    this.bookmarkClicked.emit();
  }

  onAttendingClicked() {
    this.attendingClicked.emit();
  }

  onAddSetlistBtnClicked() {
    this.addSetlistClicked.emit();
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
    this.mapElementRef = mapElement;
    if (!this.mapKit) {
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
    this.addOrMoveMarker(this.concert?.venueLongitude ?? 0, this.concert?.venueLatitude ?? 0);
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
    let venue = this.concert?.venue ?? undefined;
    let city = this.concert?.city ?? undefined;

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

  openLinkinpediaClicked() {
    if (this.concert == undefined) {
      return;
    }

    let dt = this.getDateTimeInTimezone(this.concert!.postedStartTime!, this.concert.timeZoneId!);
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

  public zoneCityLabel(timeZoneId: string | null | undefined): string {
    if (!timeZoneId) {
      return "";
    }
    const parts = timeZoneId.split("/");
    return parts[parts.length - 1].replace(/_/g, " ");
  }

  onShareClicked() {
    const link = window.location.protocol + "//" + window.location.host + "/concerts/" + this.concert?.id;
    if (navigator.share) {
      navigator.share({title: document.title, url: link}).catch(() => {});
    } else {
      navigator.clipboard.writeText(link).then(() => {
        this.messageService.add({severity: "success", summary: "Copied link to clipboard!"});
      });
    }
  }

  protected readonly ConcertTitleGenerator = ConcertTitleGenerator;
  protected readonly DateTime = DateTime;
  protected readonly String = String;
  protected readonly GetConcertBookmarkCountsResponseDto = GetConcertBookmarkCountsResponseDto;
  protected readonly environment = environment;
}
