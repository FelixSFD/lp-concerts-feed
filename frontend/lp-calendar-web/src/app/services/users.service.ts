import { Injectable } from '@angular/core';
import {Observable} from 'rxjs';
import {Guid} from 'guid-typescript';
import {UserDto, UsersService as UsersApiClient} from '../modules/lpshows-api';

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  constructor(private usersApiClient: UsersApiClient) { }


  getUsers(cached: boolean) : Observable<UserDto[]> {
    if (!cached) {
      return this.usersApiClient.getUsers(Guid.create().toString());
    }

    return this.usersApiClient.getUsers();
  }


  getUserById(id: string, cached: boolean = false) : Observable<UserDto> {
    if (!cached) {
      // disable caching
      return this.usersApiClient.getUserById(id, Guid.create().toString());
    }

    return this.usersApiClient.getUserById(id);
  }


  updateUser(user: UserDto) {
    return this.usersApiClient.updateUser(user.id ?? "", user);
  }
}
