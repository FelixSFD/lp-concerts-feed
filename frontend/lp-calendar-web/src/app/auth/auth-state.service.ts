import { Injectable } from '@angular/core';
import { BehaviorSubject} from 'rxjs';
import {OidcSecurityService, UserDataResult, LoginResponse} from 'angular-auth-oidc-client';
import {User} from '../data/users/user';
import {UserDto} from '../modules/lpshows-api';

@Injectable({ providedIn: 'root' })
export class AuthStateService {
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  private userDataSubject = new BehaviorSubject<UserDto | null>(null);
  private accessTokenSubject = new BehaviorSubject<any>(null);

  /** Public observables you can subscribe to anywhere */
  isAuthenticated$ = this.isAuthenticatedSubject.asObservable();
  userData$ = this.userDataSubject.asObservable();
  accessToken$ = this.accessTokenSubject.asObservable();

  constructor(private oidcSecurityService: OidcSecurityService) {
    // Subscribe once to the OIDC observables and propagate updates
    this.oidcSecurityService.checkAuth().subscribe((loginResponse: LoginResponse) => {
      this.isAuthenticatedSubject.next(loginResponse.isAuthenticated);
    });

    this.oidcSecurityService.userData$.subscribe((result: UserDataResult) => {
      console.debug("userData changed: ", result);
      if (result?.userData != null) {
        let user = User.fromClaims(result.userData)
        this.userDataSubject.next(user);
      } else {
        this.userDataSubject.next(null);
      }
    });

    this.oidcSecurityService.getAccessToken().subscribe(accessToken => {
      this.accessTokenSubject.next(accessToken);
    });
  }

  /** convenience getters */
  get currentUser() {
    return this.userDataSubject.value;
  }

  get isLoggedIn() {
    return this.isAuthenticatedSubject.value;
  }
}

