import { Guid } from 'guid-typescript';

class CacheParameterHelper {
  public static getRequestIdParameter(cached: boolean): string | undefined {
    return cached ? undefined : Guid.create().toString();
  }
}


export function getRequestIdParameter(cached: boolean): string | undefined {
  return CacheParameterHelper.getRequestIdParameter(cached);
}
