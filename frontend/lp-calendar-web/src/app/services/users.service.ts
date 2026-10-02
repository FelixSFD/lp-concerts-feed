import { effect, inject, Injectable, signal } from '@angular/core';
import { ProblemDetailsDto, UpdateUserProfileDto, UserDto, UsersApi } from '../modules/lpshows-api/v3';
import { environment } from '../../environments/environment';
import { addAuthentication } from '../auth/auth.config';
import { firstValueFrom } from 'rxjs';
import { OidcSecurityService } from 'angular-auth-oidc-client';

@Injectable({
  providedIn: 'root'
})
export class UsersService {
  private oidcService = inject(OidcSecurityService);
  private usersApi = inject(UsersApi);

  private currentUserSignal = signal<UserDto | null | undefined>(undefined);
  private didLoadProfileSignal = signal(false);

  private authChangedEffect = effect(async () => {
    if (this.oidcService.authenticated().isAuthenticated) {
      try {
        let current = await this.getCurrentUser();
        this.currentUserSignal.set(current);
        this.didLoadProfileSignal.set(true);
      } catch (e) {
        console.warn('Error getting current user:', e);
        let problemDetails = e as ProblemDetailsDto;
        if (problemDetails.status == 404) {
          console.info('User is logged in but has no profile yet. Setting current user to null');
          this.currentUserSignal.set(null);
          this.didLoadProfileSignal.set(true);
        }
      }
    } else {
      this.currentUserSignal.set(null);
      this.didLoadProfileSignal.set(false);
    }
  });

  constructor() {
    this.usersApi.configuration.basePath = environment.apiBaseUrl;
    addAuthentication(this.usersApi);
  }

  /**
   * Returns a readonly signal of the current user.
   */
  get currentUser() {
    return this.currentUserSignal.asReadonly();
  }

  get didLoadProfile() {
    return this.didLoadProfileSignal.asReadonly();
  }

  /**
   * Returns information about the current user.
   */
  getCurrentUser() {
    return firstValueFrom(
      this.usersApi.getCurrentUser()
    );
  }

  /**
   * Updates the profile of the current user.
   * @param request new data for the profile
   */
  updateCurrentUserProfile(request: UpdateUserProfileDto) {
    return firstValueFrom(
      this.usersApi.updateCurrentUserProfile(request)
    );
  }

  /**
   * Returns a list of suggested usernames.
   */
  getSuggestedUsernames() {
    return firstValueFrom(
      this.usersApi.getSuggestedUserNames()
    );
  }
}
