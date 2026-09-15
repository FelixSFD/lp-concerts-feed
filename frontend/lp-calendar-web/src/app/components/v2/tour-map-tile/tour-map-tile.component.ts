import {Component, Input, OnChanges, SimpleChanges} from '@angular/core';
import {RouterLink} from '@angular/router';
import {ConcertDto} from '../../../modules/lpshows-api';
import {
  projectLatitude,
  projectLongitude,
  WORLD_VIEWBOX,
} from '../../../data/world-outline';

interface TourPin {
  x: number;
  y: number;
  /** Stagger offset for the pulse, so dots don't all breathe in unison. */
  delay: number;
  /** Pulse cycle length — the highlighted pin runs a touch faster. */
  durationMs: number;
  radius: number;
  isHighlight: boolean;
  label: string;
}

/** Base pulse cycle length; the highlighted dot runs faster (see buildPins). */
const PULSE_MS = 3200;
const HIGHLIGHT_PULSE_MS = 2200;

/**
 * Dot Map
 */
@Component({
  selector: 'app-tour-map-tile',
  imports: [RouterLink],
  templateUrl: './tour-map-tile.component.html',
  styleUrl: './tour-map-tile.component.css',
})
export class TourMapTileComponent implements OnChanges {
  @Input() concerts: ConcertDto[] = [];

  /** id of the concert (nearest-to-you, falling back to next/last show) to draw as the standout dot. */
  @Input() highlightConcertId?: string;

  protected readonly viewBox = WORLD_VIEWBOX;

  pins: TourPin[] = [];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['concerts'] || changes['highlightConcertId']) {
      this.buildPins();
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

  private buildPins(): void {
    // The map page applies the same guard: 0/0 means "we don't actually know".
    const plottable = this.concerts.filter(concert =>
      concert.venueLatitude != undefined && concert.venueLatitude != 0 &&
      concert.venueLongitude != undefined && concert.venueLongitude != 0
    );


    const ordered = [...plottable].sort(
      (a, b) => a.venueLongitude! - b.venueLongitude!
    );

    const step = ordered.length > 1 ? PULSE_MS / ordered.length : 0;

    this.pins = ordered.map((concert, index) => {
      const isHighlight = this.highlightConcertId != null && concert.id === this.highlightConcertId;

      return {
        x: projectLongitude(concert.venueLongitude!),
        y: projectLatitude(concert.venueLatitude!),
        delay: Math.round(index * step),
        durationMs: isHighlight ? HIGHLIGHT_PULSE_MS : PULSE_MS,
        radius: isHighlight ? 3 : 2,
        isHighlight,
        label: concert.locationShort ?? concert.city ?? concert.venue ?? "Show",
      };
    });
  }
}
