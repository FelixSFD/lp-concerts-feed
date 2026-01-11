import { Injectable } from '@angular/core';
import {Observable} from 'rxjs';
import {Guid} from 'guid-typescript';
import {UserDto, UserNotificationSettingsDto, UsersService as UsersApiClient} from '../modules/lpshows-api';

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


  getUserNotificationSettings(userId: string, cached: boolean) : Observable<UserNotificationSettingsDto> {
    if (!cached) {
      return this.usersApiClient.getUserNotificationSettings(userId, Guid.create().toString());
    }

    return this.usersApiClient.getUserNotificationSettings(userId);
  }


  updateUserNotificationSettings(settings: UserNotificationSettingsDto) {
    return this.usersApiClient.updateUserNotificationSettings(settings.userId ?? "", settings);
  }
}
