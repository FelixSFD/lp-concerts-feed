import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot } from '@angular/router';
import { catchError, Observable, of } from 'rxjs';
import { ErrorResponseDto } from '../modules/lpshows-api';
import { StateDto } from '../modules/lpshows-api/v3';
import { LocationsService } from '../services/locations.service';

export const stateResolver: ResolveFn<StateDto | ErrorResponseDto> = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
): Observable<StateDto | ErrorResponseDto> => {
  const locationsService = inject(LocationsService);

  const countryCode = route.paramMap.get('countryCode');
  const stateCode = route.paramMap.get('stateCode');

  if (!countryCode || !stateCode) {
    return of({
      type: 'ErrorResponseDto',
      message: 'Missing country or state code'
    } as ErrorResponseDto);
  }

  return locationsService.getState(countryCode, stateCode).pipe(
    catchError(err => of(err.error as ErrorResponseDto))
  );
};
