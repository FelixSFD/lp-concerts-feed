import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable, switchMap} from 'rxjs';
import { Guid } from 'guid-typescript';
import {ConcertFilter} from '../data/concert-filter';
import {
  AdjacentConcertsResponseDto,
  ConcertDto,
  ConcertFileUploadRequestDto,
  ConcertsService as ConcertsApiClient
} from '../modules/lpshows-api';

@Injectable({
  providedIn: 'root'
})
export class ConcertsService {

  constructor(private httpClient: HttpClient, private concertsApiClient: ConcertsApiClient) { }


  /**
   * @deprecated Use getFilteredConcerts instead
   */
  getConcerts(cached: boolean, onlyFuture: boolean) : Observable<ConcertDto[]> {
    return this.getFilteredConcerts(null, cached);
  }


  getFilteredConcerts(filter: ConcertFilter | null, cached: boolean) : Observable<ConcertDto[]> {
    if (!cached) {
      // disable caching
      return this.concertsApiClient.getConcerts(filter?.tour ?? undefined, filter?.dateFrom?.toISO() ?? undefined, filter?.dateTo?.toISO() ?? undefined, Guid.create().toString(), "body", false);
    }

    return this.concertsApiClient.getConcerts(filter?.tour ?? undefined, filter?.dateFrom?.toISO() ?? undefined, filter?.dateTo?.toISO() ?? undefined, undefined, "body", false);
  }


  getNextConcert() : Observable<ConcertDto> {
    return this.concertsApiClient.getNextConcert("body", false);
  }


  getConcert(concertId: string, cached: boolean = true) : Observable<ConcertDto> {
    if (!cached) {
      // disable caching
      return this.concertsApiClient.getConcertById(concertId, Guid.create().toString(), "body", false);
    }

    return this.concertsApiClient.getConcertById(concertId, undefined, "body", false);
  }


  addConcert(concert: ConcertDto) {
    concert.status = "PUBLISHED";
    return this.concertsApiClient.addOrUpdateConcert(concert);
  }


  deleteConcert(concertId: string) {
    return this.concertsApiClient.deleteConcert(concertId);
  }


  uploadConcertSchedule(concertId: string, imageFile: File) {
    let getUrlRequest: ConcertFileUploadRequestDto = {};
    getUrlRequest.concertId = concertId;
    getUrlRequest.contentType = imageFile.type;
    getUrlRequest.type = "ConcertSchedule";

    return this.concertsApiClient.getUrlForConcertFileUpload(getUrlRequest)
      .pipe(
        switchMap((response) => {
          return this.httpClient.put(response.uploadUrl!, imageFile);
        })
      )
  }


  /**
   * Returns the previous and next ID based on the ID passed into the method
   * @param currentId ID of the current concert
   */
  getAdjacentConcerts(currentId: string) : Observable<AdjacentConcertsResponseDto> {
    return this.concertsApiClient.getAdjacentConcertsForId(currentId);
  }
}
