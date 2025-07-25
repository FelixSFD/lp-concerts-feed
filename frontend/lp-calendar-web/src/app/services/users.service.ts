import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Observable} from 'rxjs';
import {environment} from '../../environments/environment';
import {Guid} from 'guid-typescript';
import {User} from '../data/users/user';
import {Concert} from '../data/concert';

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


  getUserById(id: string, cached: boolean = false) : Observable<User> {
    let url = environment.apiCachedBaseUrl + "/Prod/users/" + id;

    if (!cached) {
      // disable caching
      const httpHeaders: HttpHeaders = new HttpHeaders({
        'Cache-Control': 'no-cache, no-store, must-revalidate, max-age=0',
        'X-LP-Request-Id': Guid.create().toString()
      });

      return this.httpClient.get<User>(url, {
        headers: httpHeaders
      });
    }

    return this.httpClient.get<User>(url);
  }


  updateUser(user: User) {
    let url = environment.apiNoCacheBaseUrl + "/Prod/users/" + user.id;
    return this.httpClient.put(url, user);
  }
}
