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

@Component({
  selector: 'app-select-venue',
  imports: [
    FormsModule,
    FloatLabel,
    Select,
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

  @Input() inputId: string = 'venueId';
  @Input() placeholder: string = '';
  @Input({ transform: booleanAttribute }) fluid: boolean = true;
  @Input({ transform: booleanAttribute }) showClear: boolean = false;
  @Input({ transform: booleanAttribute }) filter: boolean = true;
  @Input({ transform: booleanAttribute }) disabled: boolean = false;
  @Input({ transform: booleanAttribute }) invalid: boolean = false;

  @Output() venueChange = new EventEmitter<string | null>();

  countries = signal<CountryDto[]>([]);
  cities = signal<CityWithCountryDto[]>([]);
  venues = signal<VenueDto[]>([]);

  selectedCountry = signal<CountryDto | null>(null);
  selectedCityId = signal<string | null>(null);
  value = signal<string | null>(null);

  loadingCountries = signal(false);
  loadingCities = signal(false);
  loadingVenues = signal(false);

  filteredVenues = computed(() => {
    const venues = this.venues();
    const selectedCountry = this.selectedCountry();
    const selectedCityId = this.selectedCityId();

    return venues.filter((venue) => {
      if (selectedCountry && venue.countryCode !== selectedCountry.isoCode) {
        return false;
      }
      if (selectedCityId != null) {
        if (venue.cityId == null || String(venue.cityId) !== String(selectedCityId)) {
          return false;
        }
      }
      return true;
    });
  });

  private onChange: (value: string | null) => void = () => {};
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
    if (countryCode) {
      this.locationsService.getCitiesIn(countryCode).subscribe({
        next: (cities) => {
          this.cities.set(cities ?? []);
          this.loadingCities.set(false);
          const currentCityId = this.selectedCityId();
          if (currentCityId && !this.cities().some((c) => String(c.id) === String(currentCityId))) {
            this.selectedCityId.set(null);
          }
        },
        error: (err) => {
          console.error('Failed to load cities in country', err);
          this.cities.set([]);
          this.loadingCities.set(false);
        },
      });
    } else {
      this.locationsService.getCities().subscribe({
        next: (cities) => {
          this.cities.set(cities ?? []);
          this.loadingCities.set(false);
          const currentCityId = this.selectedCityId();
          if (currentCityId && !this.cities().some((c) => String(c.id) === String(currentCityId))) {
            this.selectedCityId.set(null);
          }
        },
        error: (err) => {
          console.error('Failed to load cities', err);
          this.cities.set([]);
          this.loadingCities.set(false);
        },
      });
    }
  }

  onCountryChange(country: CountryDto | null) {
    this.selectedCountry.set(country);
    this.loadCities();

    const currentValue = this.value();
    if (currentValue) {
      const currentVenue = this.venues().find((v) => v.id === currentValue);
      const selectedCountry = this.selectedCountry();
      if (currentVenue && selectedCountry && currentVenue.countryCode !== selectedCountry.isoCode) {
        this.value.set(null);
        this.onChange(null);
        this.venueChange.emit(null);
      }
    }
  }

  onCityChange(cityId: string | number | null) {
    this.selectedCityId.set(cityId != null && cityId !== '' ? String(cityId) : null);

    const currentValue = this.value();
    if (currentValue) {
      const currentVenue = this.venues().find((v) => v.id === currentValue);
      const selectedCity = this.selectedCityId();
      if (currentVenue && selectedCity != null) {
        if (currentVenue.cityId == null || String(currentVenue.cityId) !== selectedCity) {
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
      this.value.set(String(newValue));
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
      this.selectedCityId.set(null);
    } else if (typeof value === 'object' && value.id != null) {
      this.value.set(String(value.id));
      this.syncCountryAndCityFromVenue(this.value()!);
    } else {
      this.value.set(String(value));
      this.syncCountryAndCityFromVenue(this.value()!);
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

  private syncCountryAndCityFromVenue(venueId: string) {
    const venues = this.venues();
    if (!venues || venues.length === 0) {
      return;
    }
    const venue = venues.find((v) => v.id === venueId);
    if (venue) {
      if (venue.countryCode && this.selectedCountry()?.isoCode !== venue.countryCode) {
        this.selectedCountry.set(this.countries().find((c) => c.isoCode === venue.countryCode) ?? null);
        this.loadCities();
      }
      if (venue.cityId != null) {
        this.selectedCityId.set(String(venue.cityId));
      }
    }
  }
}
