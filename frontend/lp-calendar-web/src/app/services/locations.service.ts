import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {first, map, Observable} from 'rxjs';
import {OsmCity} from '../data/osm/osm-city';
import {Coordinates} from '../data/location/coordinates';
import {environment} from '../../environments/environment';
import {TimeZoneResponseDto, TimezoneService} from '../modules/lpshows-api';

/**
 * Service to retrieve location data like coordinates and timezones
 */
@Injectable({
  providedIn: 'root'
})
export class LocationsService {
  private osmApiBaseUrl = "https://nominatim.openstreetmap.org";

  constructor(private httpClient: HttpClient, private timezoneApiClient: TimezoneService) { }


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
