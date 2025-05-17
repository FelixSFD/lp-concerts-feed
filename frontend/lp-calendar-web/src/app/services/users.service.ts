import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Observable} from 'rxjs';
import {environment} from '../../environments/environment';
import {Guid} from 'guid-typescript';
import {User} from '../data/users/user';

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  constructor(private httpClient: HttpClient) { }


  getUsers(cached: boolean) : Observable<User[]> {
    let url = environment.apiCachedBaseUrl + "/Prod/users";

    if (!cached) {
      // disable caching
      const httpHeaders: HttpHeaders = new HttpHeaders({
        'Cache-Control': 'no-cache, no-store, must-revalidate, max-age=0',
        'X-LP-Request-Id': Guid.create().toString()
      });

      console.log("Headers:");
      console.log(httpHeaders);

      return this.httpClient.get<User[]>(url, {
        headers: httpHeaders
      });
    }

    return this.httpClient.get<User[]>(url);
  }
}
