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
import {ErrorResponseDto} from '../../../modules/lpshows-api';
import { load, MapKit, MarkerAnnotation } from '@apple/mapkit-loader';
import {Map as AppleMap} from 'apple-mapkit/mapkit';
import {RouterLink} from '@angular/router';
import {DiscordShareService} from '../../../services/discord-share.service';
import {DateTime} from 'luxon';
import {environment} from '../../../../environments/environment';
import {ConcertBadgesComponent} from '../concert-badges/concert-badges.component';
import {Card} from 'primeng/card';
import {SplitButton} from 'primeng/splitbutton';
import {Button} from 'primeng/button';
import {MenuItem, MessageService} from 'primeng/api';
import {Tooltip} from 'primeng/tooltip';
import {TimeSpanPipe} from '../../../data/time-span-pipe';
import {HeroCountdownComponent} from '../hero-countdown/hero-countdown.component';
import {SetlistComponent} from '../setlists/setlist/setlist.component';
import {Tag} from 'primeng/tag';
import {Image} from 'primeng/image';
import {ConcertDetailsViewModel} from './concert-details.view-model';

@Component({
  selector: 'app-concert-details',
  imports: [
    ConcertBadgesComponent,
    Card,
    SplitButton,
    Button,
    RouterLink,
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

  @Input() viewModel: ConcertDetailsViewModel | null = null;
  @Input() resolverError: ErrorResponseDto | null = null;
  @Input() isAuthenticated: boolean = false;
  @Input() canUpdateConcerts: boolean = false;
  @Input() canEditSetlists: boolean = false;
  @Input() addSetlistButtonItems: MenuItem[] = [];

  @Output() bookmarkClicked = new EventEmitter<void>();
  @Output() attendingClicked = new EventEmitter<void>();
  @Output() addSetlistClicked = new EventEmitter<void>();

  // Apple Maps
  private mapKit: MapKit | undefined;
  private appleMap: AppleMap | undefined;
  private locationMarker: MarkerAnnotation | null = null;
  private mapElementRef: ElementRef<HTMLDivElement> | undefined;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['viewModel'] && this.appleMap && this.mapKit) {
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
    if (this.viewModel?.venueCoordinates) {
      this.addOrMoveMarker(this.viewModel.venueCoordinates.longitude, this.viewModel.venueCoordinates.latitude);
    }
  }

  private zoomToCoordinates(lon: number, lat: number) {
    if (this.appleMap && this.mapKit) {
      this.appleMap.region = new this.mapKit.CoordinateRegion(
        new this.mapKit.Coordinate(lat, lon),
        new this.mapKit.CoordinateSpan(0.06, 0.2)
      );
    }
  }

  private addOrMoveMarker(lon: number, lat: number) {
    if (!this.appleMap || !this.mapKit) {
      return;
    }

    if (this.locationMarker) {
      console.debug("Pin already exists. Will just move it.");
      this.locationMarker.coordinate = new this.mapKit!.Coordinate(lat, lon);
    } else {
      console.debug("Creating new pin on the map...");
      this.locationMarker = new this.mapKit!.MarkerAnnotation(new this.mapKit!.Coordinate(lat, lon), {
        color: "#c969e0",
        map: this.appleMap,
        draggable: false
      });
      console.debug("Pin created.", this.locationMarker);
      this.appleMap?.showItems([this.locationMarker]);
    }

    this.zoomToCoordinates(lon, lat);
  }

  openLinkinpediaClicked() {
    if (!this.viewModel?.wikiUrl) {
      return;
    }

    this.tracker.trackLink(this.viewModel.wikiUrl, "link");
    window.open(this.viewModel.wikiUrl, "_blank");
  }

  onShareClicked() {
    if (!this.viewModel?.id) return;
    const link = window.location.protocol + "//" + window.location.host + "/concerts/" + this.viewModel.id;
    if (navigator.share) {
      navigator.share({title: document.title, url: link}).catch(() => {});
    } else {
      navigator.clipboard.writeText(link).then(() => {
        this.messageService.add({severity: "success", summary: "Copied link to clipboard!"});
      });
    }
  }

  protected readonly DateTime = DateTime;
}
