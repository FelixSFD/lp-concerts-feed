import { inject, Injectable } from '@angular/core';
import { UsersApi } from '../modules/lpshows-api/v3';
import { environment } from '../../environments/environment';
import { addAuthentication } from '../auth/auth.config';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UsersService {
  private usersApi = inject(UsersApi);

  constructor() {
    this.usersApi.configuration.basePath = environment.apiBaseUrl;
    addAuthentication(this.usersApi);
  }

  /**
   * Returns information about the current user.
   */
  getCurrentUser() {
    return firstValueFrom(
      this.usersApi.getCurrentUser()
    );
  }
}
