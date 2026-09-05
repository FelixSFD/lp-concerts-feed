import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of } from 'rxjs';
import { SelectVenueComponent } from './select-venue.component';
import { LocationsService } from '../../../../../services/locations.service';
import { CityWithCountryDto, CountryDto, VenueDto } from '../../../../../modules/lpshows-api/v3';

describe('SelectVenueComponent', () => {
  let component: SelectVenueComponent;
  let fixture: ComponentFixture<SelectVenueComponent>;
  let locationsService: jasmine.SpyObj<LocationsService>;

  const mockCountries: CountryDto[] = [
    { isoCode: 'DE', name: 'Germany', nativeName: 'Deutschland' },
    { isoCode: 'US', name: 'United States', nativeName: 'United States' },
  ];

  const mockAllCities: CityWithCountryDto[] = [
    { id: '1', name: 'Berlin', countryCode: 'DE', nativeName: 'Berlin', country: mockCountries[0] },
    { id: '2', name: 'Hamburg', countryCode: 'DE', nativeName: 'Hamburg', country: mockCountries[0] },
    { id: '3', name: 'New York', countryCode: 'US', nativeName: 'New York', country: mockCountries[1] },
  ];

  const mockGermanCities: CityWithCountryDto[] = [
    { id: '1', name: 'Berlin', countryCode: 'DE', nativeName: 'Berlin', country: mockCountries[0] },
    { id: '2', name: 'Hamburg', countryCode: 'DE', nativeName: 'Hamburg', country: mockCountries[0] },
  ];

  const mockUsCities: CityWithCountryDto[] = [
    { id: '3', name: 'New York', countryCode: 'US', nativeName: 'New York', country: mockCountries[1] },
  ];

  const mockVenues: VenueDto[] = [
    { id: '10', currentName: 'Uber Arena', countryCode: 'DE', cityId: '1', timeZoneId: 'Europe/Berlin' },
    { id: '20', currentName: 'Barclays Arena', countryCode: 'DE', cityId: '2', timeZoneId: 'Europe/Berlin' },
    { id: '30', currentName: 'Madison Square Garden', countryCode: 'US', cityId: '3', timeZoneId: 'America/New_York' },
  ];

  beforeEach(async () => {
    const serviceSpy = jasmine.createSpyObj('LocationsService', [
      'getCountries',
      'getVenues',
      'getCities',
      'getCitiesIn',
    ]);

    serviceSpy.getCountries.and.returnValue(of(mockCountries));
    serviceSpy.getVenues.and.returnValue(of(mockVenues));
    serviceSpy.getCities.and.returnValue(of(mockAllCities));
    serviceSpy.getCitiesIn.and.callFake((countryCode: string) => {
      if (countryCode === 'DE') {
        return of(mockGermanCities);
      }
      if (countryCode === 'US') {
        return of(mockUsCities);
      }
      return of([]);
    });

    await TestBed.configureTestingModule({
      imports: [SelectVenueComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: LocationsService, useValue: serviceSpy },
      ],
    }).compileComponents();

    locationsService = TestBed.inject(LocationsService) as jasmine.SpyObj<LocationsService>;
    fixture = TestBed.createComponent(SelectVenueComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and load initial countries, venues, and cities', () => {
    expect(component).toBeTruthy();
    expect(locationsService.getCountries).toHaveBeenCalled();
    expect(locationsService.getVenues).toHaveBeenCalled();
    expect(locationsService.getCities).toHaveBeenCalled();
    expect(component.countries()).toEqual(mockCountries);
    expect(component.venues()).toEqual(mockVenues);
    expect(component.cities()).toEqual(mockAllCities);
  });

  it('should return all venues when neither country nor city is selected', () => {
    component.selectedCountry.set(null);
    component.selectedCity.set(null);
    expect(component.filteredVenues().length).toBe(3);
  });

  it('should filter venues by country when country is selected and populate cities in that country', () => {
    component.onCountryChange(mockCountries[0]);

    expect(locationsService.getCitiesIn).toHaveBeenCalledWith('DE');
    expect(component.cities()).toEqual(mockGermanCities);
    expect(component.filteredVenues().map((v) => v.id)).toEqual(['10', '20']);
  });

  it('should filter venues by city when city is selected', () => {
    component.onCityChange(mockAllCities[0]);

    expect(component.filteredVenues().map((v) => v.id)).toEqual(['10']);
  });

  it('should filter venues by both country and city when both are selected', () => {
    component.onCountryChange(mockCountries[0]);
    component.onCityChange(mockGermanCities[1]);

    expect(component.filteredVenues().map((v) => v.id)).toEqual(['20']);
  });

  it('should clear venue value when country change invalidates current venue', () => {
    const onChangeSpy = jasmine.createSpy('onChange');
    const venueChangeSpy = spyOn(component.venueChange, 'emit');
    component.registerOnChange(onChangeSpy);

    component.value.set(mockVenues[2]); // US venue

    component.onCountryChange(mockCountries[0]); // Germany

    expect(component.value()).toBeNull();
    expect(onChangeSpy).toHaveBeenCalledWith(null);
    expect(venueChangeSpy).toHaveBeenCalledWith(null);
  });

  it('should clear venue value when city change invalidates current venue', () => {
    const onChangeSpy = jasmine.createSpy('onChange');
    const venueChangeSpy = spyOn(component.venueChange, 'emit');
    component.registerOnChange(onChangeSpy);

    component.value.set(mockVenues[0]); // Berlin venue (cityId: '1')

    component.onCityChange(mockGermanCities[1]); // Hamburg

    expect(component.value()).toBeNull();
    expect(onChangeSpy).toHaveBeenCalledWith(null);
    expect(venueChangeSpy).toHaveBeenCalledWith(null);
  });

  it('should handle ControlValueAccessor writeValue with venue object and sync country and city', () => {
    component.writeValue(mockVenues[0]);

    expect(component.value()).toEqual(mockVenues[0]);
    expect(component.selectedCountry()).toEqual(mockCountries[0]);
    expect(component.selectedCity()).toEqual(mockGermanCities[0]);
    expect(locationsService.getCitiesIn).toHaveBeenCalledWith('DE');
  });

  it('should handle ControlValueAccessor writeValue with null', () => {
    component.writeValue(null);

    expect(component.value()).toBeNull();
    expect(component.selectedCountry()).toBeNull();
    expect(component.selectedCity()).toBeNull();
  });

  it('should emit value changes and sync country and city when onVenueChange is called', () => {
    const onChangeSpy = jasmine.createSpy('onChange');
    const onTouchedSpy = jasmine.createSpy('onTouched');
    const venueChangeSpy = spyOn(component.venueChange, 'emit');

    component.registerOnChange(onChangeSpy);
    component.registerOnTouched(onTouchedSpy);

    component.onVenueChange(mockVenues[2]);

    expect(component.value()).toEqual(mockVenues[2]);
    expect(component.selectedCountry()).toEqual(mockCountries[1]);
    expect(component.selectedCity()).toEqual(mockUsCities[0]);
    expect(onChangeSpy).toHaveBeenCalledWith(mockVenues[2]);
    expect(onTouchedSpy).toHaveBeenCalled();
    expect(venueChangeSpy).toHaveBeenCalledWith(mockVenues[2]);
  });

  it('should handle setDisabledState', () => {
    component.setDisabledState(true);
    expect(component.disabled).toBeTrue();

    component.setDisabledState(false);
    expect(component.disabled).toBeFalse();
  });
});
