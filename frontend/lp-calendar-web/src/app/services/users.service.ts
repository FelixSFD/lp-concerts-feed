import { effect, inject, Injectable, signal } from '@angular/core';
import { UpdateUserProfileDto, UserDto, UsersApi } from '../modules/lpshows-api/v3';
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

  private authChangedEffect = effect(async () => {
    if (this.oidcService.authenticated()) {
      try {
        let current = await this.getCurrentUser();
        this.currentUserSignal.set(current);
      } catch (e) {
        console.warn('Error getting current user:', e);
        this.currentUserSignal.set(null);
      }
    } else {
      this.currentUserSignal.set(null);
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
}
