import {ApplicationConfig, importProvidersFrom, provideZoneChangeDetection} from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { authConfig } from './auth/auth.config';
import {authInterceptor, provideAuth} from 'angular-auth-oidc-client';
import {provideHttpClient, withInterceptors} from '@angular/common/http';
import {provideToastr, ToastrModule} from 'ngx-toastr';
import {BrowserAnimationsModule, provideAnimations} from '@angular/platform-browser/animations';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes), provideAuth(authConfig),
    provideHttpClient(withInterceptors([authInterceptor()])),
    provideAnimations(),
    provideToastr({
      positionClass: "toast-bottom-right",
      newestOnTop: false
    })
  ]
};


export const listOfShowTypes: string[] = ["Linkin Park Show", "Festival", "Other performance"];
export const defaultShowType: string = listOfShowTypes[0];
export const listOfTours: string[] = ["FROM ZERO WORLD TOUR 2025"];

export const mapAttribution: string = `<a href="${window.location.protocol}//${window.location.host}/about">Concert data by LPShows.live</a>`;
