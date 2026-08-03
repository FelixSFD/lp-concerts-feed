import {Component, Input, OnChanges, SimpleChanges} from '@angular/core';
import {RouterLink} from '@angular/router';
import {ConcertDto} from '../../../modules/lpshows-api';
import {
  projectLatitude,
  projectLongitude,
  WORLD_OUTLINE_PATHS,
  WORLD_VIEWBOX,
} from '../../../data/world-outline';

interface TourPin {
  x: number;
  y: number;
  /** Milliseconds into the sweep before this pin appears. */
  delay: number;
  label: string;
}

/** How long the whole west-to-east sweep takes. */
const SWEEP_MS = 2200;

/**
 * Every show the site knows about, dropped onto a coarse world map.
 *
 * Deliberately plain: an SVG with a fixed viewBox, so it scales itself and
 * needs no resize handling, no canvas and no animation loop. The reveal is a
 * staggered CSS animation; replaying it just re-applies the class.
 */
@Component({
  selector: 'app-tour-map-tile',
  imports: [RouterLink],
  templateUrl: './tour-map-tile.component.html',
  styleUrl: './tour-map-tile.component.css',
})
export class TourMapTileComponent implements OnChanges {
  @Input() concerts: ConcertDto[] = [];

  protected readonly viewBox = WORLD_VIEWBOX;
  protected readonly landPaths = WORLD_OUTLINE_PATHS;

  pins: TourPin[] = [];
  playing = false;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['concerts']) {
      this.buildPins();
      this.play();
    }
  }


  get countryCount(): number {
    return new Set(
      this.concerts.map(concert => concert.country).filter(country => (country?.length ?? 0) > 0)
    ).size;
  }


  get countryLabel(): string {
    const count = this.countryCount;
    return count === 1 ? "1 country" : `${count} countries`;
  }


  get showLabel(): string {
    return this.pins.length === 1 ? "1 show on the map" : `${this.pins.length} shows on the map`;
  }


  /** Restart the sweep. Dropping the class and re-adding it re-runs the CSS. */
  play(): void {
    this.playing = false;
    requestAnimationFrame(() => {
      this.playing = true;
    });
  }


  private buildPins(): void {
    // The map page applies the same guard: 0/0 means "we don't actually know".
    const plottable = this.concerts.filter(concert =>
      concert.venueLatitude != undefined && concert.venueLatitude != 0 &&
      concert.venueLongitude != undefined && concert.venueLongitude != 0
    );

    // Sweep west to east, so the reveal reads as a direction rather than noise.
    const ordered = [...plottable].sort(
      (a, b) => a.venueLongitude! - b.venueLongitude!
    );

    const step = ordered.length > 1 ? SWEEP_MS / (ordered.length - 1) : 0;

    this.pins = ordered.map((concert, index) => ({
      x: projectLongitude(concert.venueLongitude!),
      y: projectLatitude(concert.venueLatitude!),
      delay: Math.round(index * step),
      label: concert.locationShort ?? concert.city ?? concert.venue ?? "Show",
    }));
  }
}
