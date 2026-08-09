import {ResolveFn} from '@angular/router';
import {inject} from '@angular/core';
import {ErrorResponseDto} from '../modules/lpshows-api';
import {catchError, of} from 'rxjs';
import { VenueDto } from '../modules/lpshows-api/v3';
import { LocationsService } from '../services/locations.service';

export const venueResolver: ResolveFn<VenueDto | ErrorResponseDto> = (route) => {
  const locService = inject(LocationsService);
  const cityId = Number(route.paramMap.get('venueId')!);
  return locService.getVenue(cityId).pipe(
    catchError((err) => {
      let errorResponse: ErrorResponseDto = err.error;
      console.error('Failed to load venue:', errorResponse);
      return of(errorResponse);
    }),
  );
};
