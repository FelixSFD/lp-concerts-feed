import { Injectable } from '@angular/core';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {filter, map, Observable} from 'rxjs';
import {User} from '../data/users/user';
import {UserDto} from '../modules/lpshows-api';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private oidcSecurityService: OidcSecurityService) {}

  /*checkAuth() {
    this.oidcSecurityService.checkAuth().subscribe(({ isAuthenticated }) => {
      console.debug('Authenticated:', isAuthenticated);
      this.isAuthenticated$ = isAuthenticated;

      // get current user object
      this.authService.getCurrentUser().subscribe(usr => {
        console.debug("User -->", usr);
        this.currentUser$ = usr;
      });

      this.oidcSecurityService.getAccessToken().subscribe(at => {
        console.debug("ACCESS_TOKEN: " + at);
      });

      // Set user ID for Matomo tracker
      this.authService.getCurrentUser().subscribe(usr => {
        console.debug("Sending username to Matomo: ", usr);
        this.tracker.setUserId(usr.username ?? usr.id!);
      });
    });
  }*/

  getCurrentUser(): Observable<UserDto> {
    return this.oidcSecurityService.userData$.pipe(
      filter((data) => !!data?.userData),
      map((data) => User.fromClaims(data.userData))
    );
  }


  isAuthenticated(): Observable<boolean> {
    return this.oidcSecurityService.isAuthenticated()
  }
}
