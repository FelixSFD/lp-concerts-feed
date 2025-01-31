import {Component, ElementRef, inject, OnDestroy, OnInit, ViewChild} from '@angular/core';
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

@Component({
  selector: 'app-concert-details',
  imports: [
    NgIf,
    CountdownComponent,
    RouterLink,
    ConcertBadgesComponent
  ],
  templateUrl: './concert-details.component.html',
  styleUrl: './concert-details.component.css'
})
export class ConcertDetailsComponent implements OnInit {
  concert$: Concert | null = null;
  concertId: string | undefined;

  // Map of the location of the concert
  private venueMap: Map | undefined;

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

        return this.concert$ = result;
      });
  }


  @ViewChild('details')
  set watch(template: ElementRef) {
    if (template) {
      console.log("template exists, do something!");
      this.initVenueMap();
    }
  }


  private initVenueMap() {
    console.log("Initialize map...");
    if (this.venueMap != undefined) {
      console.log("Map already created. Don't add it again.");
      return;
    }

    this.venueMap = new Map({
      layers: [
        new TileLayer({
          source: new OSM(),
        }),
      ],
      target: 'venueMap',
      view: new View({
        center: [0, 0],
        zoom: 2, maxZoom: 18,
      }),
    });
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


  public getDateTimeInTimezone(inputDate: string) {
    return DateTime.fromISO(inputDate, {setZone: true});
  }


  protected readonly DateTime = DateTime;
}
