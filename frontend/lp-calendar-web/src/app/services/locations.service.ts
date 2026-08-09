import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {map, Observable} from 'rxjs';
import {OsmCity} from '../data/osm/osm-city';
import {Coordinates} from '../data/location/coordinates';
import {environment} from '../../environments/environment';
import {TimeZoneResponseDto, TimezoneService} from '../modules/lpshows-api';
import {
  CitiesApi,
  CityWithCountryDto,
  CountriesApi,
  CountryDto, CreateCityRequestDto,
  CreateCountryRequestDto, CreateStateRequestDto, CreateVenueRequestDto,
  StateDto, StateWithCountryDto, UpdateCityRequestDto,
  UpdateCountryRequestDto, UpdateStateRequestDto, UpdateVenueRequestDto, VenueDto, VenuesApi, VenueWithDetailsDto
} from '../modules/lpshows-api/v3';
import { addAuthentication } from '../auth/auth.config';

/**
 * Service to retrieve location data like coordinates and timezones
 */
@Injectable({
  providedIn: 'root'
})
export class LocationsService {
  private osmApiBaseUrl = "https://nominatim.openstreetmap.org";

  constructor(private httpClient: HttpClient, private timezoneApiClient: TimezoneService, private countriesApi: CountriesApi, private citiesApi: CitiesApi, private venuesApi: VenuesApi) {
    // Override base URL as CountriesApi uses v3
    countriesApi.configuration.basePath = environment.apiBaseUrl;
    citiesApi.configuration.basePath = environment.apiBaseUrl;
    venuesApi.configuration.basePath = environment.apiBaseUrl;
    addAuthentication(countriesApi);
    addAuthentication(citiesApi);
    addAuthentication(venuesApi);
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
  getCountry(countryCode: string): Observable<CountryDto> {
    return this.countriesApi.getCountryByIsoCode(countryCode);
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
  getState(countryCode: string, stateCode: string): Observable<StateWithCountryDto> {
    return this.countriesApi.getState(countryCode, stateCode);
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

  getCities(): Observable<CityWithCountryDto[]> {
    return this.citiesApi.getCities(undefined, "1000", undefined, ["country.name", "name"]); // TODO: filter and sorting?
  }

  getCitiesIn(countryCode: string): Observable<CityWithCountryDto[]> {
    return this.countriesApi.getCitiesInCountry(countryCode);
  }

  createCity(countryCode: string, city: CreateCityRequestDto): Observable<CityWithCountryDto> {
    return this.countriesApi.createCity(countryCode, city);
  }

  updateCity(countryCode: string, id: number, city: UpdateCityRequestDto): Observable<CityWithCountryDto> {
    return this.countriesApi.updateCity(countryCode, id, city);
  }

  deleteCity(countryCode: string, id: number): Observable<void> {
    return this.countriesApi.deleteCity(countryCode, id.toString());
  }

  getCity(countryCode: string, cityId: number): Observable<CityWithCountryDto> {
    return this.countriesApi.getCity(countryCode, cityId.toString());
  }

  /**
   * Returns a list of all venues
   */
  getVenues(): Observable<VenueDto[]> {
    return this.venuesApi.getAllVenues();
  }

  getVenue(venueId: number): Observable<VenueDto> {
    return this.venuesApi.getVenueById(venueId);
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

  getCoordinatesFor(city: string, state: string | null, country: string): Observable<Coordinates | undefined> {
    let osmUrl = this.osmApiBaseUrl + "/search.php?format=jsonv2&city=" + encodeURIComponent(city) + "&country=" + encodeURIComponent(country);
    if (state != null) {
      osmUrl = osmUrl + "&state=" + encodeURIComponent(state);
    }

    return this.getFirstCoordinateFromUrl(osmUrl);
  }


  getCoordinatesForVenue(venue: string, city: string, state: string | null, country: string): Observable<Coordinates | undefined> {
    let stateQueryComponent = state ? "," + encodeURIComponent(state) : undefined;
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


  getTimeZoneForCoordinates(lat: number, lon: number): Observable<TimeZoneResponseDto> {
    return this.timezoneApiClient.getTimeZoneByCoordinates(lat, lon);
  }
}
