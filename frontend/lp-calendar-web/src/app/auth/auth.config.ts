import {LogLevel, PassedInitialConfig} from 'angular-auth-oidc-client';
import {environment} from '../../environments/environment';
import { Configuration } from '../modules/lpshows-api/v3';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';
import { BaseService } from '../modules/lpshows-api/v3/api.base.service';

//export const apiCachedBaseUrl = "https://d1pwzjk6lcvg96.cloudfront.net";
//export const apiNoCacheBaseUrl = "https://o1qqdpvb23.execute-api.eu-central-1.amazonaws.com";

export const authConfig: PassedInitialConfig = {
  config: {
    authority: environment.cognitoBaseUrl, // Replace with your Cognito domain
    redirectUrl: environment.cognitoRedirectUrl,
    clientId: environment.cognitoClientId,
    scope: 'email openid profile', // Scopes allowed in the App Client
    responseType: 'code', // Authorization Code Flow
    silentRenew: true, // Enable silent token renewal
    useRefreshToken: true, // Use refresh tokens to maintain the session
    allowUnsafeReuseRefreshToken: true,
    secureRoutes: [
      // config was moved
    ],
    logLevel: LogLevel.Warn // Enable detailed logs for debugging
  }
}


/**
 * Patterns of routes that require authentication
 */
export const authRoutePatterns: RegExp[] = [
  /\/concerts\/(bookmarked|attending)$/,
  /\/concerts\/[^/]+\/setlists\/import$/,
  /\/concerts\/[^/]+\/bookmarks$/,
  /\/concerts\/[^/]+\/bookmarks\/status$/,
  /\/deleteConcert\//,
  /\/addConcert/,
  /\/requestFileUpload/,
  /\/users/,
  /\/users\/[^/]+/,
  /\/setlists/,
  /\/setlists\/[^/]+/,
  /\/songs/,
  /\/songs\/[^/]+/,
  /\/albums/,
  /\/albums\/[^/]+/,
  /\/mashups/,
  /\/mashups\/[^/]+/,
]


function addAuthenticationInternal(configuration: Configuration) {
  let authService = inject(AuthService);
  configuration.credentials["Bearer"] = authService.accessToken;
}

/**
 * Adds the authentication configuration to an API service or its configuration
 * @param target
 */
export function addAuthentication(target: BaseService | Configuration) {
  if (target instanceof BaseService) {
    addAuthenticationInternal(target.configuration);
  } else if (target instanceof Configuration) {
    addAuthenticationInternal(target);
  } else {
    throw new Error("Unable to add authentication. Target type unknown.");
  }
}
