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
import {defaultShowType, mapAttribution} from '../app.config';
import {defaults as defaultControls} from 'ol/control/defaults';

@Component({
  selector: 'app-tour-map-page',
  imports: [],
  templateUrl: './tour-map-page.component.html',
  styleUrl: './tour-map-page.component.css'
})
export class TourMapPageComponent implements OnInit, AfterViewInit {
  private venueMap: Map | undefined;
  private markers: Feature[] = [];


  constructor(private concertsService: ConcertsService) {
  }


  ngOnInit() {
    this.concertsService.getConcerts(true, false)
      .subscribe(results => {
        results.forEach(r => {
          if (r.venueLongitude != undefined && r.venueLongitude != 0 && r.venueLatitude != undefined && r.venueLatitude != 0) {
            let markerColor = "black";
            if (r.showType != defaultShowType && r.showType != undefined) {
              markerColor = "red";
            }
            this.addMarker(r.venueLongitude, r.venueLatitude, markerColor);
            console.log("Long: " + r.venueLongitude + "; Lat: " + r.venueLatitude);
            console.log(r);
          }
        });
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
      target: "map",
      view: new View({
        center: [0, 0],
        zoom: 2, maxZoom: 18,
      }),
    });
  }


  private addMarker(lon: number, lat: number, color: string) {
    const newCoords = fromLonLat([lon, lat]); // Convert to EPSG:3857
    console.log("Set marker at: " + newCoords.toString())

    // Create a marker feature
    const marker = new Feature({
      geometry: new Point(newCoords), // Initial position
    });

    marker.setStyle(new Style({
      image: new Icon({
        anchor: [0.5, 1],
        src: './map/map-pin-50-' + color + '.png',
        scale: 0.59
      })
    }));

    // Add marker to vector layer
    const vectorSource = new VectorSource({
      features: [marker]
    });

    const vectorLayer = new VectorLayer({
      source: vectorSource
    });

    this.venueMap?.addLayer(vectorLayer);
  }
}
