import {AfterViewInit, Component, ElementRef, inject, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {ConcertsService} from '../services/concerts.service';
import {Concert} from '../data/concert';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {DateTime} from 'luxon';
import {NgIf, NgOptimizedImage} from '@angular/common';
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

@Component({
  selector: 'app-concert-details',
  imports: [
    NgIf,
    CountdownComponent,
    RouterLink,
    ConcertBadgesComponent,
    NgOptimizedImage
  ],
  templateUrl: './concert-details.component.html',
  styleUrl: './concert-details.component.css'
})
export class ConcertDetailsComponent implements OnInit, AfterViewInit {
  concert$: Concert | null = null;
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
    this.concertId = this.route.snapshot.paramMap.get('id')!;

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
        if (result.venueLongitude != undefined && result.venueLatitude != undefined) {
          this.addOrMoveMarker(result.venueLongitude, result.venueLatitude);
        }

        return this.concert$ = result;
      });
  }


  ngAfterViewInit() {
    this.initVenueMap();
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
          color: "red",
          anchor: [0.5, 1],
          src: './map/icon.png',
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


  @ViewChild('details')
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


  public getDateTime(inputDate: string) {
    return DateTime.fromISO(inputDate, {setZone: false});
  }


  public getDateTimeInTimezone(inputDate: string, timeZoneId: string) {
    return DateTime.fromISO(inputDate, {zone: timeZoneId});
  }


  protected readonly DateTime = DateTime;
  protected readonly environment = environment;
}
