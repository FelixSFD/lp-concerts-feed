import { Injectable } from '@angular/core';
import { ConcertTypesApi, ConcertTypeDto } from '../modules/lpshows-api/v3';
import { addAuthentication } from '../auth/auth.config';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConcertTypesService {
  constructor(private concertTypesApi: ConcertTypesApi) {
    concertTypesApi.configuration.basePath = environment.apiBaseUrl;
    addAuthentication(concertTypesApi);
  }

  public getConcertTypes(cached: boolean = true) {
    // TODO: add caching parameter
    return this.concertTypesApi.getConcertTypes();
  }
}
