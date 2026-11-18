import { effect, inject, Injectable } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { filter } from 'rxjs';
import { AuthService } from '../auth/auth.service';

const returnKey = 'calendar-subscription-login-return';
const managementPath = /^\/calendar-subscriptions\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

@Injectable({ providedIn: 'root' })
export class CalendarSubscriptionNavigationService {
  private readonly oidc = inject(OidcSecurityService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly navigationEnd = toSignal(this.router.events.pipe(filter(event => event instanceof NavigationEnd)));

  private readonly returnAfterLogin = effect(() => {
    this.navigationEnd();
    if (!this.router.navigated || !this.auth.initialized() || this.auth.initializationError() || !this.oidc.authenticated().isAuthenticated) return;
    const destination = sessionStorage.getItem(returnKey);
    if (!destination) return;
    sessionStorage.removeItem(returnKey);
    if (managementPath.test(destination)) {
      void this.router.navigateByUrl(destination);
    }
  });

  login(): void {
    const path = this.router.url.split(/[?#]/, 1)[0];
    if (managementPath.test(path)) {
      sessionStorage.setItem(returnKey, path);
    } else {
      sessionStorage.removeItem(returnKey);
    }
    this.oidc.authorize();
  }
}
