import {ApplicationConfig, importProvidersFrom, provideZoneChangeDetection} from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { authConfig } from './auth/auth.config';
import {authInterceptor, provideAuth} from 'angular-auth-oidc-client';
import {provideHttpClient, withInterceptors} from '@angular/common/http';
import {provideToastr, ToastrModule} from 'ngx-toastr';
import {BrowserAnimationsModule, provideAnimations} from '@angular/platform-browser/animations';
import {provideMatomo, withRouter} from 'ngx-matomo-client';
import {environment} from '../environments/environment';
import {NgcCookieConsentConfig, provideNgcCookieConsent} from 'ngx-cookieconsent';

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
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes), provideAuth(authConfig),
    provideHttpClient(withInterceptors([authInterceptor()])),
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
    provideNgcCookieConsent(cookieConfig)
  ]
};


export const listOfShowTypes: string[] = ["Linkin Park Show", "Festival", "Other performance"];
export const defaultShowType: string = listOfShowTypes[0];
export const listOfTours: string[] = ["FROM ZERO WORLD TOUR 2024", "FROM ZERO WORLD TOUR 2025", "FROM ZERO WORLD TOUR 2026"];

export const mapAttribution: string = `<a href="${window.location.protocol}//${window.location.host}/about">Concert data by LPShows.live</a>`;
