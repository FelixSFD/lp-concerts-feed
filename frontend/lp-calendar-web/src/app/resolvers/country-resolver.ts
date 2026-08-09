import {ResolveFn} from '@angular/router';
import {inject} from '@angular/core';
import {ErrorResponseDto} from '../modules/lpshows-api';
import {catchError, of} from 'rxjs';
import { CountryDto } from '../modules/lpshows-api/v3';
import { LocationsService } from '../services/locations.service';

export const countryResolver: ResolveFn<CountryDto | ErrorResponseDto> = (route) => {
  const locService = inject(LocationsService);
  const countryCode = route.paramMap.get('countryCode')!;
  return locService.getCountry(countryCode).pipe(
    catchError((err) => {
      let errorResponse: ErrorResponseDto = err.error;
      console.error('Failed to load country:', errorResponse);
      return of(errorResponse);
    }),
  );
};
