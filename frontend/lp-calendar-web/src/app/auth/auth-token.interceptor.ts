// auth-token.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { from } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import {authRoutePatterns} from './auth.config';

export const authTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const oidcSecurityService = inject(OidcSecurityService);

  //console.log('auth for this required?', req.url);

  let authRequired = false;
  for (let i = 0; i < authRoutePatterns.length; i++) {
    let pattern = authRoutePatterns[i];
    //console.log("Testing pattern:", pattern);
    if (pattern.test(req.url)) {
      //console.log("auth will be added");
      authRequired = true;
      break;
    }
  }

  if (authRequired) {
    console.debug("Authentication will be added...");
    return from(oidcSecurityService.getAccessToken()).pipe(
      switchMap((token) => {
        const authReq = req.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`,
          },
        });
        return next(authReq);
      })
    );
  }

  // If not matched, just pass the request through
  return next(req);
};
