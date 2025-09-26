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
