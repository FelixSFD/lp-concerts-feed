import {LogLevel, PassedInitialConfig} from 'angular-auth-oidc-client';
import {provideHttpClient} from '@angular/common/http';

export const logoutRedirectUrl = "http://localhost:4200/"

export const authConfig: PassedInitialConfig = {
  config: {
    authority: 'https://cognito-idp.eu-central-1.amazonaws.com/eu-central-1_KPfToYS9T', // Replace with your Cognito domain
    redirectUrl: 'http://localhost:4200/',
    clientId: '1epkncdmjoklpcoa77pl17jatj',
    scope: 'email openid profile LpConcertsAuthServer/user_access', // Scopes allowed in the App Client
    responseType: 'code', // Authorization Code Flow
    silentRenew: true, // Enable silent token renewal
    useRefreshToken: true, // Use refresh tokens to maintain the session
    secureRoutes: ['https://o1qqdpvb23.execute-api.eu-central-1.amazonaws.com/Prod/concerts', 'https://o1qqdpvb23.execute-api.eu-central-1.amazonaws.com/Prod/timezones'],
    logLevel: LogLevel.Debug // Enable detailed logs for debugging
  }
}
