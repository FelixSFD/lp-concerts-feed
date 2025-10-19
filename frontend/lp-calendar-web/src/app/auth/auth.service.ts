import { Injectable } from '@angular/core';
import { BehaviorSubject} from 'rxjs';
import {OidcSecurityService, UserDataResult, LoginResponse} from 'angular-auth-oidc-client';
import {User} from '../data/users/user';
import {UserDto} from '../modules/lpshows-api';

@Injectable({ providedIn: 'root' })
export class AuthService {
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
      if (loginResponse.isAuthenticated) {
        this.isAuthenticatedSubject.next(loginResponse.isAuthenticated);

        this.oidcSecurityService.getAccessToken().subscribe(accessToken => {
          this.accessTokenSubject.next(accessToken);
        });
      } else {
        console.debug("Check auth failed. Not authenticated. Try refresh if token is available.");
        this.oidcSecurityService.getRefreshToken().subscribe(refreshToken => {
          console.debug("Check refresh token:", refreshToken);
          if (refreshToken) {
            // attempt a programmatic refresh using the library's refresh token flow
            console.debug("Refresh token found. Try refresh...");
            this.oidcSecurityService.forceRefreshSession().subscribe({
              next: res => console.debug('refresh succeeded', res),
              error: err => console.warn('refresh failed', err)
            });
          }
        });
      }
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
  }

  /** convenience getters */
  get currentUser() {
    return this.userDataSubject.value;
  }

  get isLoggedIn() {
    return this.isAuthenticatedSubject.value;
  }
}

