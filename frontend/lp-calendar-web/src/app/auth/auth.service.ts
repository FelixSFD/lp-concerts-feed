import {inject, Injectable} from '@angular/core';
import { BehaviorSubject} from 'rxjs';
import {OidcSecurityService, UserDataResult, LoginResponse} from 'angular-auth-oidc-client';
import {User} from '../data/users/user';
import {UserDto} from '../modules/lpshows-api';
import {UsersService} from '../services/users.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private usersService = inject(UsersService);
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
        this.handleLoginResponse(loginResponse);
      } else {
        console.debug("Check auth failed. Not authenticated. Try refresh if token is available.");
        this.oidcSecurityService.getRefreshToken().subscribe(refreshToken => {
          console.debug("Check refresh token:", refreshToken);
          if (refreshToken) {
            // attempt a programmatic refresh using the library's refresh token flow
            console.debug("Refresh token found. Try refresh...");
            this.oidcSecurityService.forceRefreshSession().subscribe({
              next: res => {
                console.debug('refresh succeeded', res);
                this.handleLoginResponse(res);
              },
              error: err => console.warn('refresh failed', err)
            });
          }
        });
      }
    });

    this.oidcSecurityService.userData$.subscribe((result: UserDataResult) => {
      console.debug("userData changed: ", result);
      if (result?.userData != null) {
        console.debug("result.userData changed: ", result?.userData);
        let user = User.fromClaims(result.userData)
        this.userDataSubject.next(user);

        if (user.id != undefined) {
          this.usersService.getUserById(user.id, true).subscribe((downloadedUser: UserDto) => {
            console.debug("fetched user from server: ", downloadedUser);
            this.userDataSubject.next(downloadedUser);
          })
        }
      } else {
        this.userDataSubject.next(null);
      }
    });
  }

  /**
   * Pushes the data from the response to the observers
   * @param loginResponse Response from library
   */
  private handleLoginResponse(loginResponse: LoginResponse) {
    this.isAuthenticatedSubject.next(loginResponse.isAuthenticated);
    this.accessTokenSubject.next(loginResponse.accessToken);
    this.userDataSubject.next(User.fromClaims(loginResponse.userData));
  }

  /** convenience getters */
  get currentUser() {
    return this.userDataSubject.value;
  }

  get isLoggedIn() {
    return this.isAuthenticatedSubject.value;
  }
}

