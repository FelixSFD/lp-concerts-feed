import {Component, ElementRef, inject, OnInit, ViewChild} from '@angular/core';
import {LegacyConcertsService} from '../../../services/legacy-concerts.service';
import {defaultShowType, listOfTours} from '../../../app.config';
import {ConcertFilter} from '../../../data/concert-filter';

import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {DateTime} from 'luxon';
import {load, Map as AppleMap, MapKit, MarkerAnnotation} from '@apple/mapkit-loader';
import {environment} from '../../../../environments/environment';
import {Card} from 'primeng/card';
import {Divider} from 'primeng/divider';
import {MessageService} from 'primeng/api';
import { SelectTourComponent } from '../admin/concerts/select-tour/select-tour.component';
import { ConcertsService } from '../../../services/concerts.service';
import { ProblemDetailsDto } from '../../../modules/lpshows-api/v3';

@Component({
  selector: 'app-tour-map-page',
  imports: [
    ReactiveFormsModule,
    Card,
    Divider,
    FormsModule,
    SelectTourComponent
  ],
  templateUrl: './tour-map-page.component.html',
  styleUrl: './tour-map-page.component.css'
})
export class TourMapPageComponent {
  private messageService = inject(MessageService);
  private concertsService = inject(ConcertsService);

  // Apple Maps
  private mapKit: MapKit | undefined;
  private appleMap: AppleMap | undefined;

  // default filter that is used when loading the list
  defaultFilter: ConcertFilter = {
    onlyFuture: false,
    dateFrom: DateTime.fromISO("0000-01-01T00:00:00.000Z"),
    dateTo: DateTime.fromISO("3000-12-31T23:59:59.999Z"),
  };

  // Filter that is used for loading the list
  currentFilter: ConcertFilter = this.defaultFilter;

  // true, if the pins on the map are being loaded from the server
  isLoadingPins$ = true;


  constructor() {
  }


  onTourSelected(event: any) {
    this.reloadPins();
  }


  @ViewChild('map')
  set appleMaps(mapElement: ElementRef<HTMLDivElement> | undefined) {
    if (!mapElement) return;
    if (!this.appleMaps) {
      console.debug('MapKit not initialized yet!');
      this.initAppleMaps().then(() => {
        this.appleMap = this.makeMap(mapElement.nativeElement);
        this.reloadPins();
      });
      return;
    }

    console.log("Will set map element: ", mapElement);
    this.appleMap = this.makeMap(mapElement.nativeElement);
    this.appleMap.showsUserLocationControl = true;
    this.appleMap.showsUserLocation = true;

    this.reloadPins();
  }


  private makeMap(mapElement: HTMLDivElement) {
    let map = new this.mapKit!.Map(mapElement);
    map.colorScheme = "adaptive";
    return map;
  }


  private async initAppleMaps() {
    this.mapKit = await load({
      token: environment.appleMapsToken,
      language: "en-US",
      libraries: ["map", "annotations", "user-location"]
    });
  }


  private async reloadPins() {
    this.isLoadingPins$ = true;

    try {
      let response = await this.concertsService.getFilteredConcerts(this.currentFilter);
      let results = response.concerts ?? [];

      let annotations = results.map(r => {
        if (r.venue.longitude != undefined && r.venue.longitude != 0 && r.venue.latitude != undefined && r.venue.latitude != 0) {
          let markerColor = "#b306d1";
          /*if (r.concertType != defaultShowType && r.concertType != undefined) {
            markerColor = "#eb4b4b";
          }*/

          return this.makeMarker(r.venue.latitude, r.venue.longitude, `${r.venue.city.name}, ${r.venue.city.country.name}`, markerColor);
        }

        return null;
      }).filter(m => m != null);

      this.appleMap?.annotations.forEach(annotation => {
        console.debug("Removing annotation", annotation);
        this.appleMap?.removeAnnotation(annotation);
        console.debug("Removed annotation");
      });

      console.debug("Showing the new annotations", annotations.length, annotations);
      this.appleMap?.showItems(annotations);
    } catch (e) {
      let problemDetails: ProblemDetailsDto = e as ProblemDetailsDto;
      this.messageService.add({
        severity: "error",
        summary: "Failed to load the concert locations",
        detail: problemDetails.detail,
      });
    } finally {
      this.isLoadingPins$ = false;
    }
  }


  private makeMarker(lat: number, lon: number, title: string | undefined, color: string): MarkerAnnotation {
    return new this.mapKit!.MarkerAnnotation(new this.mapKit!.Coordinate(lat, lon), {
      color: color,
      title: title
    });
  }

  protected readonly listOfTours = listOfTours;
}
