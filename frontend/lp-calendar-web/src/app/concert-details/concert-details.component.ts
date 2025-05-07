import {
  AfterViewInit,
  Component,
  ElementRef,
  inject,
  OnInit,
  ViewChild
} from '@angular/core';
import {ConcertsService} from '../services/concerts.service';
import {Concert} from '../data/concert';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {DateTime} from 'luxon';
import {NgIf} from '@angular/common';
import {CountdownComponent} from '../countdown/countdown.component';
import {Meta} from '@angular/platform-browser';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {ConcertBadgesComponent} from '../concert-badges/concert-badges.component';
import TileLayer from 'ol/layer/Tile';
import {OSM} from 'ol/source';
import Map from 'ol/Map';
import View from 'ol/View';
import Feature from 'ol/Feature';
import Point from 'ol/geom/Point';
import {fromLonLat} from 'ol/proj';
import Style from 'ol/style/Style';
import Icon from 'ol/style/Icon';
import VectorSource from 'ol/source/Vector';
import VectorLayer from 'ol/layer/Vector';
import {Attribution} from 'ol/control';
import {defaults as defaultControls} from 'ol/control/defaults.js';
import {mapAttribution} from '../app.config';
import {environment} from '../../environments/environment';
import {ConcertTitleGenerator} from '../data/concert-title-generator';
import {TimeSpanPipe} from '../data/time-span-pipe';
import {AdjacentConcertIdsResponse} from '../data/adjacent-concert-ids-response';
import {NgbTooltip} from '@ng-bootstrap/ng-bootstrap';
import {MatomoTracker} from 'ngx-matomo-client';

@Component({
  selector: 'app-concert-details',
  imports: [
    NgIf,
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
  tracker = inject(MatomoTracker);

  concert$: Concert | null = null;
  adjacentConcertData$: AdjacentConcertIdsResponse | null = null;
  concertId: string | undefined;

  // Map of the location of the concert
  private venueMap: Map | undefined;
  private marker: Feature | undefined;

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
    })
  }


  ngAfterViewInit() {
    this.initVenueMap();
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


  private initVenueMap() {
    const attribution = new Attribution({
      collapsible: false,
      attributions: mapAttribution,
    });

    this.venueMap = new Map({
      controls: defaultControls({attribution: false}).extend([attribution]),
      layers: [
        new TileLayer({
          source: new OSM(),
        }),
      ],
      view: new View({
        center: [0, 0],
        zoom: 2, maxZoom: 18,
      }),
    });
  }


  private zoomToCoordinates(lon: number, lat: number) {
    this.venueMap?.getView().setCenter(fromLonLat([lon, lat]));
    this.venueMap?.getView().setZoom(11);
  }


  private addOrMoveMarker(lon: number, lat: number) {
    const newCoords = fromLonLat([lon, lat]); // Convert to EPSG:3857
    console.log("Set marker at: " + newCoords.toString())

    if (this.marker == undefined) {
      // Create a marker feature
      this.marker = new Feature({
        geometry: new Point(newCoords), // Initial position
      });

      this.marker.setStyle(new Style({
        image: new Icon({
          anchor: [0.5, 1],
          src: './map/map-pin-50-black.png',
          scale: 0.59
        })
      }));

      // Add marker to vector layer
      const vectorSource = new VectorSource({
        features: [this.marker]
      });

      const vectorLayer = new VectorLayer({
        source: vectorSource
      });

      this.venueMap?.addLayer(vectorLayer);
    } else {
      (this.marker.getGeometry() as Point).setCoordinates(newCoords);
    }

    this.zoomToCoordinates(lon, lat);
  }


  @ViewChild('mapContainer')
  set detailsRendered(element: ElementRef | undefined) {
    // is called when tab rendered or destroyed
    if (element) {
      if (this.venueMap) {
        console.log("Set target");
        this.venueMap.setTarget("venueMap");
      }
    } else {
      console.log("Map tab destroyed");
      this.venueMap?.setTarget("");
    }
  }


  private updateMetaInfo(concert: Concert) {
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
}
