import { Guid } from 'guid-typescript';
import { Data } from '@angular/router';

class CacheParameterHelper {
  public static getRequestIdParameter(cached: boolean): string | undefined {
    return cached ? undefined : Guid.create().toString();
  }

  public static getUseCacheFromRouteData(data: Data): boolean {
    if (data['noCache'] === undefined) {
      return true;
    }

    return !data['noCache'];
  }
}


export function getRequestIdParameter(cached: boolean): string | undefined {
  return CacheParameterHelper.getRequestIdParameter(cached);
}

/**
 * Returns a unique ID for the request header if no cache is requested. This behavior can be configured in the route data
 * @param data Data of the route
 */
export function getUseCacheFromRouteData(data: Data): boolean {
  return CacheParameterHelper.getUseCacheFromRouteData(data);
}
