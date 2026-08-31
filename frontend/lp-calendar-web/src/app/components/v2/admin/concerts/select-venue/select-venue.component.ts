import {
  booleanAttribute,
  ChangeDetectionStrategy,
  Component,
  computed,
  EventEmitter,
  forwardRef,
  inject,
  Input,
  OnInit,
  Output,
  signal,
} from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { FloatLabel } from 'primeng/floatlabel';
import { Select } from 'primeng/select';
import { LocationsService } from '../../../../../services/locations.service';
import { CityWithCountryDto, CountryDto, VenueDto } from '../../../../../modules/lpshows-api/v3';
import { ButtonDirective } from 'primeng/button';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-select-venue',
  imports: [
    FormsModule,
    FloatLabel,
    Select,
    ButtonDirective,
    RouterLink,
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectVenueComponent),
      multi: true,
    },
  ],
  templateUrl: './select-venue.component.html',
  styleUrl: './select-venue.component.css',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class SelectVenueComponent implements ControlValueAccessor, OnInit {
  private locationsService = inject(LocationsService);

  @Input() inputId: string = 'venue';
  @Input() placeholder: string = '';
  @Input({ transform: booleanAttribute }) fluid: boolean = true;
  @Input({ transform: booleanAttribute }) showClear: boolean = false;
  @Input({ transform: booleanAttribute }) filter: boolean = true;
  @Input({ transform: booleanAttribute }) disabled: boolean = false;
  @Input({ transform: booleanAttribute }) invalid: boolean = false;

  /**
   * true, if a button to open the venue details should be shown
   */
  @Input({ transform: booleanAttribute }) openVenueButton: boolean = false;

  @Output() venueChange = new EventEmitter<VenueDto | null>();

  countries = signal<CountryDto[]>([]);
  cities = signal<CityWithCountryDto[]>([]);
  venues = signal<VenueDto[]>([]);

  selectedCountry = signal<CountryDto | null>(null);
  selectedCity = signal<CityWithCountryDto | null>(null);
  value = signal<VenueDto | null>(null);

  loadingCountries = signal(false);
  loadingCities = signal(false);
  loadingVenues = signal(false);

  filteredVenues = computed(() => {
    const venues = this.venues();
    const selectedCountry = this.selectedCountry();
    const selectedCity = this.selectedCity();

    return venues.filter((venue) => {
      if (selectedCountry && venue.countryCode !== selectedCountry.isoCode) {
        return false;
      }
      if (selectedCity != null) {
        if (venue.cityId == null || String(venue.cityId) !== String(selectedCity.id)) {
          return false;
        }
      }
      return true;
    });
  });

  private onChange: (value: VenueDto | null) => void = () => {};
  private onTouched: () => void = () => {};

  ngOnInit() {
    this.loadCountries();
    this.loadVenues();
    this.loadCities();
  }

  loadCountries() {
    this.loadingCountries.set(true);
    this.locationsService.getCountries().subscribe({
      next: (countries) => {
        this.countries.set(countries ?? []);
        this.loadingCountries.set(false);
        const currentValue = this.value();
        if (currentValue) {
          this.syncCountryAndCityFromVenue(currentValue);
        }
      },
      error: (err) => {
        console.error('Failed to load countries', err);
        this.countries.set([]);
        this.loadingCountries.set(false);
      },
    });
  }

  loadVenues() {
    this.loadingVenues.set(true);
    this.locationsService.getVenues().subscribe({
      next: (venues) => {
        this.venues.set(venues ?? []);
        this.loadingVenues.set(false);
        const currentValue = this.value();
        if (currentValue) {
          this.syncCountryAndCityFromVenue(currentValue);
        }
      },
      error: (err) => {
        console.error('Failed to load venues', err);
        this.venues.set([]);
        this.loadingVenues.set(false);
      },
    });
  }

  loadCities() {
    this.loadingCities.set(true);
    const countryCode = this.selectedCountry()?.isoCode;
    const request$ = countryCode
      ? this.locationsService.getCitiesIn(countryCode)
      : this.locationsService.getCities();

    request$.subscribe({
      next: (cities) => {
        this.cities.set(cities ?? []);
        this.loadingCities.set(false);

        const venue = this.value();
        const currentCity = this.selectedCity();

        if (venue?.cityId != null) {
          this.selectedCity.set(this.cities().find((c) => String(c.id) === String(venue.cityId)) ?? null);
        } else if (currentCity && !this.cities().some((c) => String(c.id) === String(currentCity.id))) {
          this.selectedCity.set(null);
        }
      },
      error: (err) => {
        console.error('Failed to load cities', err);
        this.cities.set([]);
        this.loadingCities.set(false);
      },
    });
  }

  onCountryChange(country: CountryDto | null) {
    this.selectedCountry.set(country);
    this.loadCities();

    const currentVenue = this.value();
    if (currentVenue) {
      const selectedCountry = this.selectedCountry();
      if (selectedCountry && currentVenue.countryCode !== selectedCountry.isoCode) {
        this.value.set(null);
        this.onChange(null);
        this.venueChange.emit(null);
      }
    }
  }

  onCityChange(city: CityWithCountryDto | null) {
    this.selectedCity.set(city);

    const currentVenue = this.value();
    if (currentVenue) {
      const selectedCity = this.selectedCity();
      if (selectedCity != null) {
        if (currentVenue.cityId == null || String(currentVenue.cityId) !== String(selectedCity.id)) {
          this.value.set(null);
          this.onChange(null);
          this.venueChange.emit(null);
        }
      }
    }
  }

  onVenueChange(newValue: any) {
    if (newValue === null || newValue === undefined || newValue === '') {
      this.value.set(null);
    } else {
      this.value.set(newValue);
      this.syncCountryAndCityFromVenue(this.value()!);
    }
    this.onChange(this.value());
    this.onTouched();
    this.venueChange.emit(this.value());
  }

  onBlur() {
    this.onTouched();
  }

  writeValue(value: any): void {
    if (value === null || value === undefined || value === '') {
      this.value.set(null);
      this.selectedCountry.set(null);
      this.selectedCity.set(null);
    } else if (typeof value === 'object' && value.id != null) {
      this.value.set(value);
      this.syncCountryAndCityFromVenue(this.value()!);
    } else {
      const foundVenue = this.venues().find((v) => String(v.id) === String(value));
      if (foundVenue) {
        this.value.set(foundVenue);
        this.syncCountryAndCityFromVenue(foundVenue);
      } else {
        this.value.set(value);
        this.syncCountryAndCityFromVenue(this.value()!);
      }
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  private syncCountryAndCityFromVenue(venue: VenueDto) {
    if (venue) {
      if (venue.countryCode && this.selectedCountry()?.isoCode !== venue.countryCode) {
        this.selectedCountry.set(this.countries().find((c) => c.isoCode === venue.countryCode) ?? null);
        this.loadCities();
      } else if (venue.cityId != null) {
        this.selectedCity.set(this.cities().find((c) => String(c.id) === String(venue.cityId)) ?? null);
      }
    }
  }
}
