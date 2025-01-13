import { Injectable } from '@angular/core';
import { Concert } from '../data/concert';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConcertsService {

  constructor(private httpClient: HttpClient) { }


  getConcerts() : Observable<Concert[]> {
    let url = "https://o1qqdpvb23.execute-api.eu-central-1.amazonaws.com/Prod/concerts";
    return this.httpClient.get<Concert[]>(url);
  }


  addConcert(concert: Concert) {
    let url = "https://o1qqdpvb23.execute-api.eu-central-1.amazonaws.com/Prod/concerts";
    return this.httpClient.put(url, concert);
  }
}
