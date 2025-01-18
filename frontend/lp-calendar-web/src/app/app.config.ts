import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { authConfig } from './auth/auth.config';
import {authInterceptor, provideAuth} from 'angular-auth-oidc-client';
import {provideHttpClient, withInterceptors} from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [provideZoneChangeDetection({ eventCoalescing: true }), provideRouter(routes), provideAuth(authConfig), provideHttpClient(withInterceptors([authInterceptor()]))]
};


//export const apiCachedBaseUrl = "https://d1pwzjk6lcvg96.cloudfront.net";
//export const apiNoCacheBaseUrl = "https://o1qqdpvb23.execute-api.eu-central-1.amazonaws.com";
