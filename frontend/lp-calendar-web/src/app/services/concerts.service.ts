import { Injectable } from '@angular/core';
import { Concert } from '../data/concert';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Observable} from 'rxjs';
import {DeleteConcertRequest} from '../data/delete-concert-request';
import {apiBaseUrl} from '../app.config';

@Injectable({
  providedIn: 'root'
})
export class ConcertsService {

  constructor(private httpClient: HttpClient) { }


  getConcerts() : Observable<Concert[]> {
    let url = apiBaseUrl + "/Prod/concerts";
    return this.httpClient.get<Concert[]>(url);
  }


  addConcert(concert: Concert) {
    let url = apiBaseUrl + "/Prod/concerts";
    return this.httpClient.put(url, concert);
  }


  deleteConcert(concertId: string) {
    let deleteRequest = new DeleteConcertRequest();
    deleteRequest.concertId = concertId;

    const options = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
      }),
      body: deleteRequest,
    };

    let url = apiBaseUrl + "/Prod/concerts";
    return this.httpClient.delete(url, options);
  }
}
