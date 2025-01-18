import { Injectable } from '@angular/core';
import { Concert } from '../data/concert';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Observable} from 'rxjs';
import {DeleteConcertRequest} from '../data/delete-concert-request';
import {authConfig} from '../auth/auth.config';
import {environment} from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ConcertsService {

  constructor(private httpClient: HttpClient) { }


  getConcerts() : Observable<Concert[]> {
    let url = environment.apiCachedBaseUrl + "/Prod/concerts";
    return this.httpClient.get<Concert[]>(url);
  }


  getConcert(concertId: string) : Observable<Concert> {
    let url = environment.apiCachedBaseUrl + "/Prod/concerts?id=" + concertId;
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
}
