import { Injectable } from '@angular/core';
import { Concert } from '../data/concert';
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
    let url = environment.apiCachedBaseUrl + "/concerts";

    let queryString = filter != null ? this.getQueryStringForFilter(filter) : "";
    if (queryString.length > 0) {
      url += "?" + queryString;
    }

    if (!cached) {
      // disable caching
      const httpHeaders: HttpHeaders = new HttpHeaders({
        'Cache-Control': 'no-cache, no-store, must-revalidate, max-age=0',
        'X-LP-Request-Id': Guid.create().toString()
      });

      console.log("Headers:");
      console.log(httpHeaders);

      return this.concertsApiClient.getConcerts(filter?.tour ?? undefined, filter?.dateFrom?.toISO() ?? undefined, filter?.dateTo?.toISO() ?? undefined, Guid.create().toString(), "body", false);
    }

    return this.concertsApiClient.getConcerts(filter?.tour ?? undefined, filter?.dateFrom?.toISO() ?? undefined, filter?.dateTo?.toISO() ?? undefined, undefined, "body", false);
  }


  getNextConcert() : Observable<Concert> {
    let url = environment.apiCachedBaseUrl + "/concerts/next";
    return this.httpClient.get<Concert>(url);
  }


  getConcert(concertId: string, cached: boolean = true) : Observable<Concert> {
    let url = environment.apiCachedBaseUrl + "/concerts/" + concertId;
    if (!cached) {
      // disable caching
      const httpHeaders: HttpHeaders = new HttpHeaders({
        'Cache-Control': 'no-cache, no-store, must-revalidate, max-age=0',
        'X-LP-Request-Id': Guid.create().toString()
      });

      console.log("Headers:");
      console.log(httpHeaders);

      return this.httpClient.get<Concert>(url, {
        headers: httpHeaders
      });
    }

    return this.httpClient.get<Concert>(url);
  }


  addConcert(concert: Concert) {
    let url = environment.apiNoCacheBaseUrl + "/addConcert";
    return this.httpClient.put(url, concert);
  }


  deleteConcert(concertId: string) {
    let deleteRequest = new DeleteConcertRequest();
    deleteRequest.concertId = concertId;

    console.log(authConfig);

    let url = environment.apiNoCacheBaseUrl + "/deleteConcert/" + concertId;
    //let url = "https://o1qqdpvb23.execute-api.eu-central-1.amazonaws.com/Prod/concerts/" + concertId + "/delete";
    return this.httpClient.delete(url);
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
