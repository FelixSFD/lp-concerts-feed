import {Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import {ConcertsService} from '../services/concerts.service';
import {defaultShowType, listOfTours,} from '../app.config';
import {ConcertFilter} from '../data/concert-filter';
import {ToastrService} from 'ngx-toastr';

import {ReactiveFormsModule} from '@angular/forms';
import {DateTime} from 'luxon';
import {load, Map as AppleMap, MapKit, MarkerAnnotation} from '@apple/mapkit-loader';
import {environment} from '../../environments/environment';

@Component({
  selector: 'app-tour-map-page',
  imports: [
    ReactiveFormsModule
],
  templateUrl: './tour-map-page.component.html',
  styleUrl: './tour-map-page.component.css'
})
export class TourMapPageComponent {
  // Apple Maps
  private mapKit: MapKit | undefined;
  private appleMap: AppleMap | undefined;

  // default filter that is used when loading the list
  defaultFilter: ConcertFilter = {
    onlyFuture: false,
    tour: "FROM ZERO WORLD TOUR 2026",
    dateFrom: DateTime.fromISO("0000-01-01T00:00:00.000Z"),
    dateTo: DateTime.fromISO("3000-12-31T23:59:59.999Z"),
  };

  // Filter that is used for loading the list
  currentFilter: ConcertFilter = this.defaultFilter;

  // true, if the pins on the map are being loaded from the server
  isLoadingPins$ = true;


  constructor(private concertsService: ConcertsService, private toastrService: ToastrService) {
  }


  onTourSelected(event: any) {
    let tour = event.target.value;
    if (tour == "ALL") {
      this.currentFilter.tour = null;
    } else {
      this.currentFilter.tour = tour;
    }
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


  private reloadPins() {
    this.isLoadingPins$ = true;

    this.concertsService.getFilteredConcerts(this.currentFilter, true)
      .subscribe({
        next: results => {
          this.isLoadingPins$ = false;

          let annotations = results.map(r => {
            if (r.venueLongitude != undefined && r.venueLongitude != 0 && r.venueLatitude != undefined && r.venueLatitude != 0) {
              let markerColor = "#b306d1";
              if (r.showType != defaultShowType && r.showType != undefined) {
                markerColor = "#eb4b4b";
              }

              return this.makeMarker(r.venueLatitude, r.venueLongitude, r.locationShort ?? r.city ?? undefined, markerColor);
            }

            return null;
          }).filter(m => m != null);

          this.appleMap?.showItems(annotations);
        },
        error: err => {
          this.isLoadingPins$ = false;
          this.toastrService.error(err.message, "Failed to load pins");
        }
      });
  }


  private makeMarker(lat: number, lon: number, title: string | undefined, color: string): MarkerAnnotation {
    return new this.mapKit!.MarkerAnnotation(new this.mapKit!.Coordinate(lat, lon), {
      color: color,
      title: title
    });
  }

  protected readonly listOfTours = listOfTours;
}
