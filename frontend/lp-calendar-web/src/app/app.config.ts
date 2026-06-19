import {ApplicationConfig, LOCALE_ID, provideZoneChangeDetection} from '@angular/core';
import {provideRouter} from '@angular/router';

import { routes } from './app.routes';
import { authConfig } from './auth/auth.config';
import {
  AbstractSecurityStorage,
  DefaultLocalStorageService,
  provideAuth
} from 'angular-auth-oidc-client';
import {provideHttpClient, withInterceptors} from '@angular/common/http';
import {provideToastr} from 'ngx-toastr';
import {provideAnimations} from '@angular/platform-browser/animations';
import {provideMatomo, withRouter} from 'ngx-matomo-client';
import {environment} from '../environments/environment';
import {NgcCookieConsentConfig, provideNgcCookieConsent} from 'ngx-cookieconsent';
import {BASE_PATH} from './modules/lpshows-api';
import {authTokenInterceptor} from './auth/auth-token.interceptor';
import {providePrimeNG} from 'primeng/config';
import lpshowsPreset from '../lpshows-preset';
import {ConfirmationService, MessageService} from 'primeng/api';
import {TourConfig} from './data/tour-config';

const cookieConfig:NgcCookieConsentConfig = {
  "cookie": {
    "domain": window.location.hostname
  },
  "position": "bottom",
  "theme": "classic",
  "palette": {
    "popup": {
      "background": "#000000",
      "text": "#ffffff",
      "link": "#ffffff"
    },
    "button": {
      "background": "#bc00a1",
      "text": "#000000",
      "border": "transparent"
    }
  },
  "type": "opt-out",
  "content": {
    "message": "This website uses cookies to ensure you get the best experience on our website and to be able to improve the website based on anonymous web analytics.",
    "dismiss": "Got it!",
    "deny": "Refuse cookies",
    "link": "Learn more",
    "href": "/privacy",
    "policy": "Cookie Policy"
  }
};

export const appConfig: ApplicationConfig = {
  providers: [
    {
      provide: LOCALE_ID,
      useFactory: () => navigator.language
    },
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAuth(authConfig),
    { provide: AbstractSecurityStorage, useClass: DefaultLocalStorageService },
    provideHttpClient(withInterceptors([authTokenInterceptor])),
    provideAnimations(),
    provideToastr({
      positionClass: "toast-bottom-right",
      newestOnTop: false
    }),
    provideMatomo(
      {
        siteId: environment.trackingSiteId,
        trackerUrl: environment.trackingUrl,
      },
      withRouter({
        delay: 1000
      }),
    ),
    provideNgcCookieConsent(cookieConfig),
    providePrimeNG({
      theme: {
        preset: lpshowsPreset
      },
      ripple: true,
      translation: {
        dateFormat: "dd.mm.yy"
      }
    }),
    ConfirmationService,
    MessageService,
    { provide: BASE_PATH, useValue: environment.apiCachedBaseUrl } // base path for API
  ]
};


export const listOfShowTypes: string[] = ["Linkin Park Show", "Festival", "Other performance"];
export const defaultShowType: string = listOfShowTypes[0];
export const tourConfigs: TourConfig[] = [
  {
    label: "FROM ZERO WORLD TOUR 2024",
    value: "FROM ZERO WORLD TOUR 2024",
  },
  {
    label: "FROM ZERO WORLD TOUR 2025",
    value: "FROM ZERO WORLD TOUR 2025",
  },
  {
    label: "FROM ZERO WORLD TOUR 2026",
    value: "FROM ZERO WORLD TOUR 2026",
  }
]

export const listOfTours: string[] = ["FROM ZERO WORLD TOUR 2024", "FROM ZERO WORLD TOUR 2025", "FROM ZERO WORLD TOUR 2026"];

export const mapAttribution: string = `<a href="${window.location.protocol}//${window.location.host}/about">Concert data by LPShows.live</a>`;
