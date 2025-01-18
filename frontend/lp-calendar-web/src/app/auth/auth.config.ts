import {LogLevel, PassedInitialConfig} from 'angular-auth-oidc-client';
import {provideHttpClient} from '@angular/common/http';
import {environment} from '../../environments/environment';

export const logoutRedirectUrl = "http://localhost:4200/"

//export const apiCachedBaseUrl = "https://d1pwzjk6lcvg96.cloudfront.net";
//export const apiNoCacheBaseUrl = "https://o1qqdpvb23.execute-api.eu-central-1.amazonaws.com";

export const authConfig: PassedInitialConfig = {
  config: {
    authority: environment.cognitoBaseUrl, // Replace with your Cognito domain
    redirectUrl: environment.cognitoRedirectUrl,
    clientId: environment.cognitoClientId,
    scope: 'email openid profile lpcalendar-dev_LpConcertsAuthServer/user_access', // Scopes allowed in the App Client
    responseType: 'code', // Authorization Code Flow
    silentRenew: true, // Enable silent token renewal
    useRefreshToken: true, // Use refresh tokens to maintain the session
    secureRoutes: [environment.apiNoCacheBaseUrl + '/Prod/deleteConcert/', environment.apiNoCacheBaseUrl + '/Prod/addConcert'],
    logLevel: LogLevel.Debug // Enable detailed logs for debugging
  }
}
