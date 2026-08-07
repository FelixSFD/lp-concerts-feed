import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {map, Observable} from 'rxjs';
import {OsmCity} from '../data/osm/osm-city';
import {Coordinates} from '../data/location/coordinates';
import {environment} from '../../environments/environment';
import {TimeZoneResponseDto, TimezoneService} from '../modules/lpshows-api';
import { CountriesApi, CountryDto, CreateCountryRequestDto, UpdateCountryRequestDto } from '../modules/lpshows-api/v3';
import { addAuthentication } from '../auth/auth.config';

/**
 * Service to retrieve location data like coordinates and timezones
 */
@Injectable({
  providedIn: 'root'
})
export class LocationsService {
  private osmApiBaseUrl = "https://nominatim.openstreetmap.org";

  constructor(private httpClient: HttpClient, private timezoneApiClient: TimezoneService, private countriesApi: CountriesApi) {
    // Override base URL as CountriesApi uses v3
    countriesApi.configuration.basePath = environment.apiBaseUrl;
    addAuthentication(countriesApi);
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
