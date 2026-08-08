import {ResolveFn} from '@angular/router';
import {inject} from '@angular/core';
import {ErrorResponseDto} from '../modules/lpshows-api';
import {catchError, of} from 'rxjs';
import { CityWithCountryDto } from '../modules/lpshows-api/v3';
import { LocationsService } from '../services/locations.service';

export const cityResolver: ResolveFn<CityWithCountryDto | ErrorResponseDto> = (route) => {
  const locService = inject(LocationsService);
  const countryCode = route.paramMap.get('countryCode')!;
  const cityId = Number(route.paramMap.get('cityId')!);
  return locService.getCity(countryCode, cityId).pipe(
    catchError((err) => {
      let errorResponse: ErrorResponseDto = err.error;
      console.error('Failed to load city:', errorResponse);
      return of(errorResponse);
    }),
  );
};
