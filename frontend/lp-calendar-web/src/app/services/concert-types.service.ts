import { Service } from '@angular/core';
import { ConcertTypesApi } from '../modules/lpshows-api/v3';
import { addAuthentication } from '../auth/auth.config';

@Service()
export class ConcertTypesService {
  constructor(private concertTypesApi: ConcertTypesApi) {
    addAuthentication(concertTypesApi);
  }


  public getConcertTypes(cached: boolean = true) {
    // TODO: add caching parameter
    return this.concertTypesApi.getConcertTypes();
  }
}
