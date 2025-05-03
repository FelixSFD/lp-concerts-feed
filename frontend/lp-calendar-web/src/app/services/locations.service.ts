import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {first, map, Observable} from 'rxjs';
import {OsmCity} from '../data/osm/osm-city';
import {Coordinates} from '../data/location/coordinates';

/**
 * Service to retrieve location data like coordinates and timezones
 */
@Injectable({
  providedIn: 'root'
})
export class LocationsService {

  constructor(private httpClient: HttpClient) { }


  getCoordinatesFor(city: string, state: string | null, country: string): Observable<Coordinates | undefined> {
    let osmUrl = "https://nominatim.openstreetmap.org/search.php?format=jsonv2&city=" + encodeURIComponent(city) + "&country=" + encodeURIComponent(country);
    if (state != null) {
      osmUrl = osmUrl + "&state=" + encodeURIComponent(state);
    }

    return this.httpClient.get<OsmCity[]>(osmUrl).pipe(
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
}
