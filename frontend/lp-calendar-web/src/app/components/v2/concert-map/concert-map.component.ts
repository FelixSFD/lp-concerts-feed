import {
  ChangeDetectionStrategy,
  Component,
  ElementRef, Input,
  OnChanges,
  OnDestroy, OnInit, signal,
  SimpleChanges,
  ViewChild
} from '@angular/core';
import { load, MapKit, MarkerAnnotation } from '@apple/mapkit-loader';
import {Map as AppleMap} from 'apple-mapkit/mapkit';
import { environment } from '../../../../environments/environment';
import { defaultShowTypeId } from '../../../app.config';

@Component({
  imports: [],
  selector: 'app-concert-map',
  styleUrl: './concert-map.component.css',
  templateUrl: './concert-map.component.html',
})
export class ConcertMapComponent implements OnInit, OnDestroy {
  // Apple Maps
  private mapKit: MapKit | undefined;
  private mapKitPromise: Promise<MapKit> | null = null;
  private appleMap: AppleMap | undefined;
  private mapElementRef: ElementRef<HTMLDivElement> | undefined;

  @Input("pins")
  public pins = signal<ConcertMapPin[]>([]);

  ngOnInit() {
    this.fillMapData(this.pins());
  }

  ngOnDestroy(): void {
    if (this.appleMap) {
      this.appleMap.destroy();
      this.appleMap = undefined;
    }
  }

  private async initAppleMaps(): Promise<MapKit> {
    if (this.mapKit) {
      return this.mapKit;
    }
    if (!this.mapKitPromise) {
      this.mapKitPromise = load({
        token: environment.appleMapsToken,
        language: "en-US",
        libraries: ["map", "annotations"],
      });
    }
    this.mapKit = await this.mapKitPromise;
    return this.mapKit;
  }

  @ViewChild('appleMaps')
  set appleMaps(mapElement: ElementRef<HTMLDivElement> | undefined) {
    if (!mapElement) {
      if (this.appleMap) {
        this.appleMap.destroy();
        this.appleMap = undefined;
      }
      this.mapElementRef = undefined;
      return;
    }

    if (this.appleMap && this.mapElementRef?.nativeElement !== mapElement.nativeElement) {
      this.appleMap.destroy();
      this.appleMap = undefined;
    }

    this.mapElementRef = mapElement;

    if (!this.mapKit) {
      console.debug('MapKit not initialized yet!');
      this.initAppleMaps().then(() => {
        if (this.mapElementRef?.nativeElement === mapElement.nativeElement && !this.appleMap) {
          this.appleMap = this.makeMap(mapElement.nativeElement);
          this.fillMapData(this.pins());
        }
      });
      return;
    }

    if (!this.appleMap) {
      console.log("Will set map element: ", mapElement);
      this.appleMap = this.makeMap(mapElement.nativeElement);
    }
    this.fillMapData(this.pins());
  }

  private makeMap(mapElement: HTMLDivElement) {
    let map = new this.mapKit!.Map(mapElement);
    map.colorScheme = "adaptive";
    return map;
  }

  private fillMapData(pins: ConcertMapPin[]) {
    let annotations = pins.map(p => {
      if (p.longitude != undefined && p.longitude != 0 && p.latitude != undefined && p.latitude != 0) {
        let markerColor = p.color ?? "#c969e0";

        return this.makeMarker(p.latitude, p.longitude, p.label, markerColor);
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
  }

  private makeMarker(lat: number, lon: number, title: string | undefined, color: string): MarkerAnnotation {
    return new this.mapKit!.MarkerAnnotation(new this.mapKit!.Coordinate(lat, lon), {
      color: color,
      title: title
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
}

export class ConcertMapPin {
  public color?: string;
  public label?: string;
  public latitude!: number;
  public longitude!: number;
}
