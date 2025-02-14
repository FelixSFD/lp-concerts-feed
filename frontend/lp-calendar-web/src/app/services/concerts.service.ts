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

@Injectable({
  providedIn: 'root'
})
export class ConcertsService {

  constructor(private httpClient: HttpClient) { }


  getConcerts(cached: boolean) : Observable<Concert[]> {
    let url = environment.apiCachedBaseUrl + "/Prod/concerts";

    if (!cached) {
      // disable caching
      const httpHeaders: HttpHeaders = new HttpHeaders({
        'Cache-Control': 'no-cache, no-store, must-revalidate, max-age=0',
        'X-LP-Request-Id': Guid.create().toString()
      });

      console.log("Headers:");
      console.log(httpHeaders);

      return this.httpClient.get<Concert[]>(url, {
        headers: httpHeaders
      });
    }

    return this.httpClient.get<Concert[]>(url);
  }


  getNextConcert() : Observable<Concert> {
    let url = environment.apiCachedBaseUrl + "/Prod/concerts/next";
    return this.httpClient.get<Concert>(url);
  }


  getConcert(concertId: string, cached: boolean = true) : Observable<Concert> {
    let url = environment.apiCachedBaseUrl + "/Prod/concerts/" + concertId;
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
    let url = environment.apiNoCacheBaseUrl + "/Prod/addConcert";
    return this.httpClient.put(url, concert);
  }


  deleteConcert(concertId: string) {
    let deleteRequest = new DeleteConcertRequest();
    deleteRequest.concertId = concertId;

    console.log(authConfig);

    let url = environment.apiNoCacheBaseUrl + "/Prod/deleteConcert/" + concertId;
    //let url = "https://o1qqdpvb23.execute-api.eu-central-1.amazonaws.com/Prod/concerts/" + concertId + "/delete";
    return this.httpClient.delete(url);
  }


  uploadConcertSchedule(concertId: string, imageFile: File) {
    let requestUrl = environment.apiNoCacheBaseUrl + "/Prod/requestFileUpload";
    let getUrlRequest = new GetS3UploadUrlRequest();
    getUrlRequest.concertId = concertId;
    getUrlRequest.contentType = "image/png";
    getUrlRequest.type = "ConcertSchedule";

    return this.httpClient.put<GetS3UploadUrlResponse>(requestUrl, getUrlRequest)
      .pipe(
        switchMap((response) => {
          return this.httpClient.put(response.uploadUrl!, imageFile);
        })
      )
  }
}
