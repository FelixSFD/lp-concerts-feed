import {ResolveFn} from '@angular/router';
import {inject} from '@angular/core';
import {ErrorResponseDto} from '../modules/lpshows-api';
import {catchError, of} from 'rxjs';
import { VenueDto, VenueWithDetailsDto } from '../modules/lpshows-api/v3';
import { LocationsService } from '../services/locations.service';
import { getUseCacheFromRouteData } from '../helper/cache-parameter-helper';

export const venueResolver: ResolveFn<VenueDto | ErrorResponseDto> = (route) => {
  const locService = inject(LocationsService);
  const cityId = Number(route.paramMap.get('venueId')!);
  return locService.getVenue(cityId, getUseCacheFromRouteData(route.data)).pipe(
    catchError((err) => {
      let errorResponse: ErrorResponseDto = err.error;
      console.error('Failed to load venue:', errorResponse);
      return of(errorResponse);
    }),
  );
};

export const venueDetailsResolver: ResolveFn<VenueWithDetailsDto | ErrorResponseDto> = (route) => {
  const locService = inject(LocationsService);
  const cityId = Number(route.paramMap.get('venueId')!);
  return locService.getVenueDetails(cityId, getUseCacheFromRouteData(route.data)).pipe(
    catchError((err) => {
      let errorResponse: ErrorResponseDto = err.error;
      console.error('Failed to load venue details:', errorResponse);
      return of(errorResponse);
    }),
  );
};
