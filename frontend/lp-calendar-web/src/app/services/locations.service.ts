import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import { firstValueFrom, map, Observable, of, switchMap } from 'rxjs';
import {OsmCity} from '../data/osm/osm-city';
import {Coordinates} from '../data/location/coordinates';
import {environment} from '../../environments/environment';
import {
  AddVenueNameRequestDto,
  CitiesApi, CityListResponseDto,
  CityWithCountryDto,
  CountriesApi,
  CountryDto,
  CreateCityRequestDto,
  CreateCountryRequestDto,
  CreateStateRequestDto,
  CreateVenueRequestDto,
  StateDto,
  StateWithCountryDto, TimeZoneDto, TimezonesApi,
  UpdateCityRequestDto,
  UpdateCountryRequestDto,
  UpdateStateRequestDto,
  UpdateVenueNameRequestDto,
  UpdateVenueRequestDto,
  VenueDto, VenueListResponseDto,
  VenuesApi,
  VenueWithDetailsDto
} from '../modules/lpshows-api/v3';
import { addAuthentication } from '../auth/auth.config';
import { getRequestIdParameter } from '../helper/cache-parameter-helper';
import { VenueFilter } from '../data/venue-filter';

/**
 * Service to retrieve location data like coordinates and timezones
 */
@Injectable({
  providedIn: 'root'
})
export class LocationsService {
  private osmApiBaseUrl = "https://nominatim.openstreetmap.org";

  constructor(private httpClient: HttpClient, private timezoneApi: TimezonesApi, private countriesApi: CountriesApi, private citiesApi: CitiesApi, private venuesApi: VenuesApi) {
    // Override base URL as CountriesApi uses v3
    countriesApi.configuration.basePath = environment.apiBaseUrl;
    citiesApi.configuration.basePath = environment.apiBaseUrl;
    venuesApi.configuration.basePath = environment.apiBaseUrl;
    timezoneApi.configuration.basePath = environment.apiBaseUrl;
    addAuthentication(countriesApi);
    addAuthentication(citiesApi);
    addAuthentication(venuesApi);
    addAuthentication(timezoneApi);
  }

  /**
   * Returns a list of all countries
   */
  getCountries(): Observable<CountryDto[]> {
    return this.countriesApi.getCountries();
  }


  /**
   * Returns a list of all countries
   */
  getCountry(countryCode: string, cached: boolean = true): Observable<CountryDto> {
    return this.countriesApi.getCountryByIsoCode(countryCode, getRequestIdParameter(cached));
  }

  /**
   * Creates a new country
   * @param country
   */
  createCountry(country: CreateCountryRequestDto): Observable<CountryDto> {
    return this.countriesApi.createCountry(country);
  }

  /**
   * Updates a country
   */
  updateCountry(countryCode: string, country: UpdateCountryRequestDto): Observable<CountryDto> {
    return this.countriesApi.updateCountry(countryCode, country);
  }

  /**
   * Deletes the country
   * @param countryCode
   */
  deleteCountry(countryCode: string): Observable<any> {
    return this.countriesApi.deleteCountryByIsoCode(countryCode);
  }

  /**
   * Returns a list of all states
   */
  getStatesIn(countryCode: string): Observable<StateWithCountryDto[]> {
    return this.countriesApi.getStatesInCountry(countryCode);
  }

  /**
   * Returns a single state
   */
  getState(countryCode: string, stateCode: string, cached: boolean = true): Observable<StateWithCountryDto> {
    return this.countriesApi.getState(countryCode, stateCode, getRequestIdParameter(cached));
  }

  /**
   * Creates a new state
   */
  createState(countryCode: string, state: CreateStateRequestDto): Observable<StateDto> {
    return this.countriesApi.createState(countryCode, state);
  }

  /**
   * Updates a state
   */
  updateState(countryCode: string, stateCode: string, state: UpdateStateRequestDto): Observable<StateDto> {
    return this.countriesApi.updateState(countryCode, stateCode, state);
  }

  /**
   * Deletes a state
   */
  deleteState(countryCode: string, stateCode: string): Observable<any> {
    return this.countriesApi.deleteState(countryCode, stateCode);
  }

  async getCities(cached: boolean = true): Promise<CityWithCountryDto[]> {
    let filter: VenueFilter = {
      orderBy: ["country.name", "name"],
    };
    let all = await this.getFilteredCities(filter, 1000, 0, cached);
    return all.cities ?? [];
  }

  getFilteredCities(filter: VenueFilter, limit: number = 100, skip: number = 0, cached: boolean = true): Promise<CityListResponseDto> {
    return firstValueFrom(this.citiesApi.getCities(getRequestIdParameter(cached),
      filter.countryCode,
      limit,
      skip,
      filter.orderBy)
    );
  }

  getCitiesIn(countryCode: string): Promise<CityWithCountryDto[]> {
    return firstValueFrom(this.countriesApi.getCitiesInCountry(countryCode));
  }

  createCity(countryCode: string, city: CreateCityRequestDto): Observable<CityWithCountryDto> {
    return this.countriesApi.createCity(countryCode, city);
  }

  updateCity(countryCode: string, id: number, city: UpdateCityRequestDto): Observable<CityWithCountryDto> {
    return this.countriesApi.updateCity(countryCode, id, city);
  }

  deleteCity(countryCode: string, id: number): Observable<void> {
    return this.countriesApi.deleteCity(countryCode, id);
  }

  getCity(countryCode: string, cityId: number, cached: boolean = true): Observable<CityWithCountryDto> {
    return this.countriesApi.getCity(countryCode, cityId, getRequestIdParameter(cached));
  }

  /**
   * Returns a list of all venues
   */
  async getVenues(): Promise<VenueDto[]> {
    let all = await this.getFilteredVenues({}, 10000);
    return all.venues ?? [];
  }

  getFilteredVenues(filter: VenueFilter, limit: number = 100, skip: number = 0, cached: boolean = true): Promise<VenueListResponseDto> {
    return firstValueFrom(this.venuesApi.getVenues(getRequestIdParameter(cached),
      filter.countryCode,
      filter.venue,
      limit,
      skip,
      filter.orderBy)
    );
  }

  getVenue(venueId: number, cached: boolean = true): Observable<VenueDto> {
    return this.venuesApi.getVenueById(venueId, getRequestIdParameter(cached));
  }

  getVenueDetails(venueId: number, cached: boolean = true): Observable<VenueWithDetailsDto> {
    return this.venuesApi.getVenueWithDetailsById(venueId, getRequestIdParameter(cached));
  }

  /**
   * Creates a new venue
   * @param venue
   */
  createVenue(venue: CreateVenueRequestDto): Observable<VenueDto> {
    return this.venuesApi.createVenue(venue);
  }

  /**
   * Updates a venue
   */
  updateVenue(id: number, venue: UpdateVenueRequestDto): Observable<VenueWithDetailsDto> {
    return this.venuesApi.updateVenue(id, venue);
  }

  deleteVenue(id: number): Observable<void> {
    return this.venuesApi.deleteVenueById(id);
  }

  /**
   * Adds a new name to a venue for a given time range
   */
  addNewVenueName(venueId: number, request: AddVenueNameRequestDto): Observable<VenueWithDetailsDto> {
    return this.venuesApi.addNewVenueName(venueId, request);
  }

  /**
   * Updates an existing name of a venue
   */
  updateVenueName(venueId: number, venueNameId: number, request: UpdateVenueNameRequestDto): Observable<any> {
    return this.venuesApi.updateVenueName(venueId, venueNameId, request);
  }

  /**
   * Deletes a previous name of a venue
   */
  deleteVenueName(venueId: number, venueNameId: number): Observable<any> {
    return this.venuesApi.deleteVenueName(venueId, venueNameId);
  }

  getCoordinatesFor(city: string, state: string | null, country: string): Observable<Coordinates | undefined> {
    let osmUrl = this.osmApiBaseUrl + "/search.php?format=jsonv2&city=" + encodeURIComponent(city) + "&country=" + encodeURIComponent(country);
    if (state != null) {
      osmUrl = osmUrl + "&state=" + encodeURIComponent(state);
    }

    return this.getFirstCoordinateFromUrl(osmUrl);
  }


  getCoordinatesForVenue(venue: string, city: string, state: string | null, country: string): Observable<Coordinates | undefined> {
    let stateQueryComponent = state ? "," + encodeURIComponent(state) : "";
    let osmUrl = this.osmApiBaseUrl + "/search.php?format=jsonv2&q=" + encodeURIComponent(venue) + stateQueryComponent + "," + encodeURIComponent(city) + "," + encodeURIComponent(country);
    return this.getFirstCoordinateFromUrl(osmUrl);
  }


  private getFirstCoordinateFromUrl(url: string) {
    return this.httpClient.get<OsmCity[]>(url).pipe(
      map(
        places => {
          let osmCity = places.find(place => place.lat != null && place.lon != null);
          if (osmCity == undefined) {
            return undefined;
          }

          return new Coordinates(parseFloat(osmCity.lat!), parseFloat(osmCity.lon!));
        }
      )
    );
  }


  getTimeZoneForCoordinates(lat: number, lon: number): Observable<TimeZoneDto> {
    return this.timezoneApi.getTimeZoneByCoordinates(lat, lon);
  }


  getTimeZoneForCity(cityName: string, state: string | null, country: string): Observable<TimeZoneDto | undefined> {
    return this.getCoordinatesFor(cityName, state, country)
      .pipe(
        switchMap(coordinates => {
          if (!coordinates) {
            return of(undefined);
          }
          return this.getTimeZoneForCoordinates(coordinates.latitude, coordinates.longitude);
        })
      );
  }
}
