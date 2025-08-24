import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Observable, switchMap} from 'rxjs';
import {DeleteConcertRequest} from '../data/delete-concert-request';
import {authConfig} from '../auth/auth.config';
import {environment} from '../../environments/environment';
import { Guid } from 'guid-typescript';
import {GetS3UploadUrlRequest} from '../data/get-s3-upload-url-request';
import {GetS3UploadUrlResponse} from '../data/get-s3-upload-url-response';
import {AdjacentConcertIdsResponse} from '../data/adjacent-concert-ids-response';
import {ConcertFilter} from '../data/concert-filter';
import {ConcertDto, ConcertsService as ConcertsApiClient} from '../modules/lpshows-api';

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
    let requestUrl = environment.apiNoCacheBaseUrl + "/requestFileUpload";
    let getUrlRequest = new GetS3UploadUrlRequest();
    getUrlRequest.concertId = concertId;
    getUrlRequest.contentType = imageFile.type;
    getUrlRequest.type = "ConcertSchedule";

    return this.httpClient.put<GetS3UploadUrlResponse>(requestUrl, getUrlRequest)
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
  getAdjacentConcerts(currentId: string) : Observable<AdjacentConcertIdsResponse> {
    let url = environment.apiCachedBaseUrl + "/concerts/" + currentId + "/adjacent";
    return this.httpClient.get<AdjacentConcertIdsResponse>(url);
  }


  private getQueryStringForFilter(filter: ConcertFilter) {
    let queryStringParts: string[] = [];
    if (filter.tour != undefined) {
      queryStringParts.push(`tour=${encodeURIComponent(filter.tour ?? "null")}`);
    }

    if (!filter.onlyFuture) {
      queryStringParts.push(`only_future=false`);
    }

    // add the parameters for the date range
    if (filter.dateFrom != null && filter.dateFrom?.isValid) {
      queryStringParts.push(`date_from=${encodeURIComponent(filter.dateFrom.toISO() ?? "")}`);
    }
    if (filter.dateTo != null && filter.dateTo?.isValid) {
      queryStringParts.push(`date_to=${encodeURIComponent(filter.dateTo.toISO() ?? "")}`);
    }

    return queryStringParts.join('&');
  }
}
