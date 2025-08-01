import {AfterViewInit, Component, OnInit} from '@angular/core';
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
import {ConcertsService} from '../services/concerts.service';
import {Attribution} from 'ol/control';
import {defaultShowType, listOfTours, mapAttribution} from '../app.config';
import {defaults as defaultControls} from 'ol/control/defaults';
import {ConcertFilterComponent} from '../concert-filter/concert-filter.component';
import {ConcertFilter} from '../data/concert-filter';
import {ToastrService} from 'ngx-toastr';
import {NgForOf, NgIf} from '@angular/common';
import {ReactiveFormsModule} from '@angular/forms';

@Component({
  selector: 'app-tour-map-page',
  imports: [
    ConcertFilterComponent,
    NgIf,
    NgForOf,
    ReactiveFormsModule
  ],
  templateUrl: './tour-map-page.component.html',
  styleUrl: './tour-map-page.component.css'
})
export class TourMapPageComponent implements OnInit, AfterViewInit {
  private venueMap: Map | undefined;
  private markerFeatures: Feature<Point>[] = [];
  private vectorSource: VectorSource<Feature<Point>> | undefined;
  private vectorLayer: VectorLayer | undefined;

  // default filter that is used when loading the list
  defaultFilter: ConcertFilter = {
    onlyFuture: false,
    tour: "FROM ZERO WORLD TOUR 2025"
  };

  // Filter that is used for loading the list
  currentFilter: ConcertFilter = this.defaultFilter;

  // true, if the pins on the map are being loaded from the server
  isLoadingPins$ = true;


  constructor(private concertsService: ConcertsService, private toastrService: ToastrService) {
  }


  ngOnInit() {
    this.reloadPins();
  }


  ngAfterViewInit() {
    this.initVenueMap();
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


  private reloadPins() {
    this.isLoadingPins$ = true;

    this.concertsService.getFilteredConcerts(this.currentFilter, true)
      .subscribe({
        next: results => {
          this.isLoadingPins$ = false;

          this.clearAllPins();

          results.forEach(r => {
            if (r.venueLongitude != undefined && r.venueLongitude != 0 && r.venueLatitude != undefined && r.venueLatitude != 0) {
              let markerColor = "black";
              if (r.showType != defaultShowType && r.showType != undefined) {
                markerColor = "red";
              }
              this.addMarker(r.venueLongitude, r.venueLatitude, markerColor);
            }
          });
        },
        error: err => {
          this.isLoadingPins$ = false;
          this.toastrService.error(err.message, "Failed to load pins");
        }
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
      target: "map",
      view: new View({
        center: [0, 0],
        zoom: 2, maxZoom: 18,
      }),
    });
  }


  private addMarker(lon: number, lat: number, color: string) {
    const newCoords = fromLonLat([lon, lat]); // Convert to EPSG:3857
    console.debug("Set marker at: " + newCoords.toString())

    // make sure the vectorSource and layer exist
    if (this.vectorLayer == undefined) {
      this.vectorSource = new VectorSource({
        features: []
      });
    }

    // Create vector layer if it doesn't exist yet
    if (this.vectorLayer == undefined) {
      this.vectorLayer = new VectorLayer({
        source: this.vectorSource
      });

      this.venueMap?.addLayer(this.vectorLayer);
    }

    // Create a marker feature
    const markerFeature = new Feature({
      geometry: new Point(newCoords), // Initial position
    });

    markerFeature.setStyle(new Style({
      image: new Icon({
        anchor: [0.5, 1],
        src: './map/map-pin-50-' + color + '.png',
        scale: 0.59
      })
    }));

    // Add marker to vector layer
    this.vectorSource?.addFeature(markerFeature);
    this.markerFeatures.push(markerFeature);
  }


  /**
   * Removes all pins from the map
   */
  private clearAllPins() {
    this.vectorSource?.removeFeatures(this.markerFeatures);
    this.markerFeatures = [];
  }

  protected readonly listOfTours = listOfTours;
}
